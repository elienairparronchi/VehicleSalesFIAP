param(
    [string]$ApiBaseUrl = "http://localhost:5000",
    [string]$KeycloakBaseUrl = "http://localhost:8081",
    [string]$KeycloakAdminUsername = "admin",
    [string]$KeycloakAdminPassword = "admin"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-Condition {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-HttpStatus {
    param([scriptblock]$Request)

    try {
        $response = & $Request
        return [int]$response.StatusCode
    }
    catch {
        if ($null -ne $_.Exception.Response -and $null -ne $_.Exception.Response.StatusCode) {
            return [int]$_.Exception.Response.StatusCode
        }

        throw
    }
}

function Wait-ForApi {
    for ($attempt = 1; $attempt -le 60; $attempt++) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri "$ApiBaseUrl/health" -TimeoutSec 5
            if ([int]$response.StatusCode -eq 200) {
                return
            }
        }
        catch {
            Write-Host "Waiting for API health ($attempt/60)..."
        }

        Start-Sleep -Seconds 5
    }

    throw "The API did not become healthy within five minutes."
}

function Assert-PricesAreAscending {
    param(
        [object[]]$Vehicles,
        [string]$ListName
    )

    for ($index = 1; $index -lt $Vehicles.Count; $index++) {
        $previous = [decimal]$Vehicles[$index - 1].price
        $current = [decimal]$Vehicles[$index].price
        Assert-Condition ($previous -le $current) "$ListName is not ordered by ascending price."
    }
}

Write-Host "Waiting for the Docker Compose stack..."
Wait-ForApi

$realm = "vehiclesalesfiap"
$clientId = "vehiclesalesfiap-api"
$managerUsername = "vehicle.manager"
$managerPassword = "VehicleManager123!"
$buyerUsername = "e2e.buyer.$([Guid]::NewGuid().ToString('N').Substring(0, 12))"
$buyerPassword = "E2eBuyer123!"
$adminToken = $null
$createdUserLocation = $null

try {
    Write-Host "Authenticating the local Keycloak administrator..."
    $adminTokenResponse = Invoke-RestMethod -Method Post `
        -Uri "$KeycloakBaseUrl/realms/master/protocol/openid-connect/token" `
        -ContentType "application/x-www-form-urlencoded" `
        -Body @{
            grant_type = "password"
            client_id = "admin-cli"
            username = $KeycloakAdminUsername
            password = $KeycloakAdminPassword
        }
    $adminToken = $adminTokenResponse.access_token

    Write-Host "Registering a new buyer in Keycloak..."
    $createUserResponse = Invoke-WebRequest -UseBasicParsing -Method Post `
        -Uri "$KeycloakBaseUrl/admin/realms/$realm/users" `
        -Headers @{ Authorization = "Bearer $adminToken" } `
        -ContentType "application/json" `
        -Body (@{
            username = $buyerUsername
            email = "$buyerUsername@example.com"
            firstName = "E2E"
            lastName = "Buyer"
            enabled = $true
            emailVerified = $true
            credentials = @(
                @{
                    type = "password"
                    value = $buyerPassword
                    temporary = $false
                }
            )
        } | ConvertTo-Json -Depth 5)
    Assert-Condition ([int]$createUserResponse.StatusCode -eq 201) "Keycloak did not create the buyer."
    $createdUserLocation = [string]$createUserResponse.Headers.Location

    Write-Host "Obtaining manager and buyer access tokens..."
    $managerToken = (Invoke-RestMethod -Method Post `
        -Uri "$KeycloakBaseUrl/realms/$realm/protocol/openid-connect/token" `
        -ContentType "application/x-www-form-urlencoded" `
        -Body @{
            grant_type = "password"
            client_id = $clientId
            username = $managerUsername
            password = $managerPassword
        }).access_token
    $buyerToken = (Invoke-RestMethod -Method Post `
        -Uri "$KeycloakBaseUrl/realms/$realm/protocol/openid-connect/token" `
        -ContentType "application/x-www-form-urlencoded" `
        -Body @{
            grant_type = "password"
            client_id = $clientId
            username = $buyerUsername
            password = $buyerPassword
        }).access_token

    $managerHeaders = @{ Authorization = "Bearer $managerToken" }
    $buyerHeaders = @{ Authorization = "Bearer $buyerToken" }
    $sampleVehicleJson = @{
        brand = "Access Test"
        model = "Unauthorized"
        year = 2025
        color = "Black"
        price = 50000
    } | ConvertTo-Json

    Write-Host "Checking endpoint authorization..."
    $anonymousCreateStatus = Invoke-HttpStatus {
        Invoke-WebRequest -UseBasicParsing -Method Post `
            -Uri "$ApiBaseUrl/api/v1/vehicles" `
            -ContentType "application/json" `
            -Body $sampleVehicleJson
    }
    $buyerCreateStatus = Invoke-HttpStatus {
        Invoke-WebRequest -UseBasicParsing -Method Post `
            -Uri "$ApiBaseUrl/api/v1/vehicles" `
            -Headers $buyerHeaders `
            -ContentType "application/json" `
            -Body $sampleVehicleJson
    }
    Assert-Condition ($anonymousCreateStatus -eq 401) "Anonymous vehicle creation should return HTTP 401."
    Assert-Condition ($buyerCreateStatus -eq 403) "Buyer vehicle creation should return HTTP 403."

    Write-Host "Creating vehicles out of price order..."
    $expensiveVehicle = Invoke-RestMethod -Method Post `
        -Uri "$ApiBaseUrl/api/v1/vehicles" `
        -Headers $managerHeaders `
        -ContentType "application/json" `
        -Body (@{
            brand = "Volkswagen"
            model = "T-Cross"
            year = 2025
            color = "Gray"
            price = 145000
        } | ConvertTo-Json)
    $affordableVehicle = Invoke-RestMethod -Method Post `
        -Uri "$ApiBaseUrl/api/v1/vehicles" `
        -Headers $managerHeaders `
        -ContentType "application/json" `
        -Body (@{
            brand = "Fiat"
            model = "Pulse"
            year = 2024
            color = "Red"
            price = 105000
        } | ConvertTo-Json)

    $availableResponse = Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/api/v1/vehicles/available"
    $availableVehicles = @($availableResponse | ForEach-Object { $_ })
    Assert-PricesAreAscending $availableVehicles "Available vehicle list"
    $createdAvailableVehicles = @(
        $availableVehicles | Where-Object {
            $_.id -eq $affordableVehicle.id -or $_.id -eq $expensiveVehicle.id
        }
    )
    Assert-Condition ($createdAvailableVehicles.Count -eq 2) "Created vehicles were not returned as available."
    Assert-Condition ($createdAvailableVehicles[0].id -eq $affordableVehicle.id) "Created vehicles are not ordered by price."

    Write-Host "Checking purchase authorization and completing purchases..."
    $managerPurchaseStatus = Invoke-HttpStatus {
        Invoke-WebRequest -UseBasicParsing -Method Post `
            -Uri "$ApiBaseUrl/api/v1/vehicles/$($affordableVehicle.id)/purchase" `
            -Headers $managerHeaders
    }
    Assert-Condition ($managerPurchaseStatus -eq 403) "Manager purchase should return HTTP 403."

    $affordableSale = Invoke-RestMethod -Method Post `
        -Uri "$ApiBaseUrl/api/v1/vehicles/$($affordableVehicle.id)/purchase" `
        -Headers $buyerHeaders
    $expensiveSale = Invoke-RestMethod -Method Post `
        -Uri "$ApiBaseUrl/api/v1/vehicles/$($expensiveVehicle.id)/purchase" `
        -Headers $buyerHeaders
    Assert-Condition ($affordableSale.vehicleId -eq $affordableVehicle.id) "Affordable vehicle purchase was not persisted."
    Assert-Condition ($expensiveSale.vehicleId -eq $expensiveVehicle.id) "Expensive vehicle purchase was not persisted."
    Assert-Condition ([decimal]$affordableSale.purchasePrice -eq 105000) "Purchase price snapshot is incorrect."

    $purchasedVehicle = Invoke-RestMethod -Method Get `
        -Uri "$ApiBaseUrl/api/v1/vehicles/$($affordableVehicle.id)"
    Assert-Condition ($purchasedVehicle.status -eq "Sold") "Purchased vehicle was not marked as sold."

    $soldResponse = Invoke-RestMethod -Method Get `
        -Uri "$ApiBaseUrl/api/v1/vehicles/sold" `
        -Headers $managerHeaders
    $soldVehicles = @($soldResponse | ForEach-Object { $_ })
    Assert-PricesAreAscending $soldVehicles "Sold vehicle list"
    $createdSoldVehicles = @(
        $soldVehicles | Where-Object {
            $_.id -eq $affordableVehicle.id -or $_.id -eq $expensiveVehicle.id
        }
    )
    Assert-Condition ($createdSoldVehicles.Count -eq 2) "Purchased vehicles were not returned as sold."
    Assert-Condition ($createdSoldVehicles[0].id -eq $affordableVehicle.id) "Sold vehicles are not ordered by price."

    $secondPurchaseStatus = Invoke-HttpStatus {
        Invoke-WebRequest -UseBasicParsing -Method Post `
            -Uri "$ApiBaseUrl/api/v1/vehicles/$($affordableVehicle.id)/purchase" `
            -Headers $buyerHeaders
    }
    Assert-Condition ($secondPurchaseStatus -eq 400) "A sequential second purchase should return HTTP 400."

    Write-Host "Docker Compose end-to-end smoke test passed."
}
finally {
    if ($null -ne $adminToken -and -not [string]::IsNullOrWhiteSpace($createdUserLocation)) {
        try {
            Invoke-WebRequest -UseBasicParsing -Method Delete `
                -Uri $createdUserLocation `
                -Headers @{ Authorization = "Bearer $adminToken" } | Out-Null
        }
        catch {
            Write-Warning "The temporary Keycloak buyer could not be removed."
        }
    }
}
