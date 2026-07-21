from __future__ import annotations

import html
import re
import textwrap
from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.platypus import PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "docs" / "final-submission.md"
OUTPUT = ROOT / "output" / "pdf" / "VehicleSalesFIAP-entrega-final.pdf"

PAGE_WIDTH, _ = A4
MARGIN = 1.8 * cm
USABLE_WIDTH = PAGE_WIDTH - (2 * MARGIN)


def build_styles():
    styles = getSampleStyleSheet()
    styles.add(
        ParagraphStyle(
            name="CoverTitle",
            parent=styles["Title"],
            fontName="Helvetica-Bold",
            fontSize=22,
            leading=28,
            alignment=TA_CENTER,
            textColor=colors.HexColor("#B91C1C"),
            spaceAfter=14,
        )
    )
    styles.add(
        ParagraphStyle(
            name="CoverSub",
            parent=styles["Normal"],
            fontName="Helvetica",
            fontSize=11,
            leading=16,
            alignment=TA_CENTER,
            textColor=colors.HexColor("#334155"),
            spaceAfter=8,
        )
    )
    styles.add(
        ParagraphStyle(
            name="H1Custom",
            parent=styles["Heading1"],
            fontName="Helvetica-Bold",
            fontSize=16,
            leading=20,
            textColor=colors.HexColor("#B91C1C"),
            spaceBefore=12,
            spaceAfter=8,
        )
    )
    styles.add(
        ParagraphStyle(
            name="H2Custom",
            parent=styles["Heading2"],
            fontName="Helvetica-Bold",
            fontSize=12.5,
            leading=16,
            textColor=colors.HexColor("#111827"),
            spaceBefore=10,
            spaceAfter=6,
        )
    )
    styles.add(
        ParagraphStyle(
            name="BodyCustom",
            parent=styles["BodyText"],
            fontName="Helvetica",
            fontSize=9.2,
            leading=13,
            textColor=colors.HexColor("#111827"),
            spaceAfter=6,
        )
    )
    styles.add(
        ParagraphStyle(
            name="BulletCustom",
            parent=styles["BodyText"],
            fontName="Helvetica",
            fontSize=9.2,
            leading=13,
            leftIndent=14,
            firstLineIndent=-8,
            textColor=colors.HexColor("#111827"),
            spaceAfter=4,
        )
    )
    styles.add(
        ParagraphStyle(
            name="CodeCustom",
            parent=styles["BodyText"],
            fontName="Courier",
            fontSize=7.5,
            leading=10,
            textColor=colors.HexColor("#0F172A"),
            backColor=colors.HexColor("#F8FAFC"),
            borderPadding=5,
            spaceBefore=4,
            spaceAfter=7,
        )
    )
    styles.add(
        ParagraphStyle(
            name="TableHeader",
            parent=styles["BodyText"],
            fontName="Helvetica-Bold",
            fontSize=8,
            leading=10,
            textColor=colors.white,
        )
    )
    styles.add(
        ParagraphStyle(
            name="TableCell",
            parent=styles["BodyText"],
            fontName="Helvetica",
            fontSize=7.8,
            leading=9.5,
            textColor=colors.HexColor("#111827"),
        )
    )
    styles.add(
        ParagraphStyle(
            name="Notice",
            parent=styles["BodyText"],
            fontName="Helvetica-Bold",
            fontSize=9,
            leading=13,
            textColor=colors.HexColor("#7F1D1D"),
            backColor=colors.HexColor("#FEF2F2"),
            borderColor=colors.HexColor("#FCA5A5"),
            borderWidth=0.5,
            borderPadding=7,
            spaceBefore=6,
            spaceAfter=8,
        )
    )
    return styles


STYLES = build_styles()


def inline_markup(value: str) -> str:
    escaped = html.escape(value)
    escaped = re.sub(r"`([^`]+)`", r'<font name="Courier">\1</font>', escaped)
    return re.sub(
        r"(https?://[^\s<]+)",
        r'<link href="\1"><font color="#B91C1C">\1</font></link>',
        escaped,
    )


def make_para(value: str, style_name: str = "BodyCustom") -> Paragraph:
    return Paragraph(inline_markup(value), STYLES[style_name])


def make_code_block(value: str) -> Paragraph:
    wrapped_lines: list[str] = []
    for raw_line in value.rstrip("\n").splitlines():
        if len(raw_line) <= 96:
            wrapped_lines.append(raw_line)
        else:
            wrapped_lines.extend(
                textwrap.wrap(raw_line, width=96, replace_whitespace=False, drop_whitespace=False) or [""]
            )

    code = "<br/>".join(html.escape(line).replace(" ", "&nbsp;") for line in wrapped_lines)
    return Paragraph(code or "&nbsp;", STYLES["CodeCustom"])


def make_table(rows: list[str]) -> Table | None:
    clean_rows: list[list[str]] = []
    for row in rows:
        cells = [cell.strip() for cell in row.strip().strip("|").split("|")]
        if all(set(cell.replace(" ", "")) <= {"-", ":"} for cell in cells):
            continue
        clean_rows.append(cells)

    if not clean_rows:
        return None

    max_cols = max(len(row) for row in clean_rows)
    for row in clean_rows:
        row.extend([""] * (max_cols - len(row)))

    if max_cols == 2:
        col_widths = [USABLE_WIDTH * 0.38, USABLE_WIDTH * 0.62]
    elif max_cols == 3:
        col_widths = [USABLE_WIDTH * 0.28, USABLE_WIDTH * 0.34, USABLE_WIDTH * 0.38]
    else:
        col_widths = [USABLE_WIDTH / max_cols] * max_cols

    table_data = []
    for row_index, row in enumerate(clean_rows):
        style = STYLES["TableHeader"] if row_index == 0 else STYLES["TableCell"]
        table_data.append([Paragraph(inline_markup(cell), style) for cell in row])

    table = Table(table_data, colWidths=col_widths, hAlign="LEFT", repeatRows=1)
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#B91C1C")),
                ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
                ("GRID", (0, 0), (-1, -1), 0.35, colors.HexColor("#CBD5E1")),
                ("BACKGROUND", (0, 1), (-1, -1), colors.white),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 6),
                ("RIGHTPADDING", (0, 0), (-1, -1), 6),
                ("TOPPADDING", (0, 0), (-1, -1), 5),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 5),
            ]
        )
    )
    return table


def flush_paragraph(story: list, buffer: list[str]) -> None:
    if buffer:
        story.append(make_para(" ".join(buffer).strip()))
        buffer.clear()


def add_cover(story: list) -> None:
    story.append(Spacer(1, 1.8 * cm))
    story.append(Paragraph("VehicleSalesFIAP", STYLES["CoverTitle"]))
    story.append(Paragraph("Tech Challenge FIAP/SOAT - Fase 3", STYLES["CoverSub"]))
    story.append(Paragraph("API para plataforma de revenda de veiculos", STYLES["CoverSub"]))
    story.append(Spacer(1, 0.8 * cm))

    table = Table(
        [
            [
                Paragraph("Repositorio", STYLES["TableHeader"]),
                Paragraph(
                    '<link href="https://github.com/elienairparronchi/VehicleSalesFIAP">'
                    '<font color="#B91C1C">https://github.com/elienairparronchi/VehicleSalesFIAP</font>'
                    "</link>",
                    STYLES["TableCell"],
                ),
            ],
            [
                Paragraph("Video", STYLES["TableHeader"]),
                Paragraph("PENDENTE - substituir pelo link do video apos gravacao e publicacao", STYLES["TableCell"]),
            ],
            [
                Paragraph("Imagem GHCR", STYLES["TableHeader"]),
                Paragraph("ghcr.io/elienairparronchi/vehiclesalesfiap", STYLES["TableCell"]),
            ],
        ],
        colWidths=[USABLE_WIDTH * 0.25, USABLE_WIDTH * 0.75],
        hAlign="CENTER",
    )
    table.setStyle(
        TableStyle(
            [
                ("GRID", (0, 0), (-1, -1), 0.35, colors.HexColor("#CBD5E1")),
                ("BACKGROUND", (0, 0), (0, -1), colors.HexColor("#B91C1C")),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 7),
                ("RIGHTPADDING", (0, 0), (-1, -1), 7),
                ("TOPPADDING", (0, 0), (-1, -1), 7),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
            ]
        )
    )
    story.append(table)
    story.append(Spacer(1, 0.7 * cm))
    story.append(
        Paragraph(
            "Observacao: antes do envio oficial, substitua o campo do video pelo link publicado.",
            STYLES["Notice"],
        )
    )
    story.append(PageBreak())


def parse_markdown(text: str) -> list:
    story: list = []
    add_cover(story)

    paragraph_buffer: list[str] = []
    code_buffer: list[str] = []
    table_buffer: list[str] = []
    in_code = False
    skip_first_h1 = True

    for line in text.splitlines():
        stripped = line.strip()

        if stripped.startswith("```"):
            if in_code:
                story.append(make_code_block("\n".join(code_buffer)))
                code_buffer.clear()
                in_code = False
            else:
                flush_paragraph(story, paragraph_buffer)
                if table_buffer:
                    table = make_table(table_buffer)
                    if table:
                        story.append(table)
                        story.append(Spacer(1, 7))
                    table_buffer.clear()
                in_code = True
            continue

        if in_code:
            code_buffer.append(line)
            continue

        if stripped.startswith("|") and stripped.endswith("|"):
            flush_paragraph(story, paragraph_buffer)
            table_buffer.append(stripped)
            continue

        if table_buffer:
            table = make_table(table_buffer)
            if table:
                story.append(table)
                story.append(Spacer(1, 7))
            table_buffer.clear()

        if not stripped:
            flush_paragraph(story, paragraph_buffer)
            continue

        if stripped.startswith("# "):
            flush_paragraph(story, paragraph_buffer)
            if skip_first_h1:
                skip_first_h1 = False
                continue
            story.append(make_para(stripped[2:].strip(), "H1Custom"))
        elif stripped.startswith("## "):
            flush_paragraph(story, paragraph_buffer)
            story.append(make_para(stripped[3:].strip(), "H1Custom"))
        elif stripped.startswith("### "):
            flush_paragraph(story, paragraph_buffer)
            story.append(make_para(stripped[4:].strip(), "H2Custom"))
        elif stripped.startswith("- "):
            flush_paragraph(story, paragraph_buffer)
            story.append(Paragraph("- " + inline_markup(stripped[2:].strip()), STYLES["BulletCustom"]))
        elif re.match(r"^\d+\.\s+", stripped):
            flush_paragraph(story, paragraph_buffer)
            story.append(make_para(stripped, "BulletCustom"))
        else:
            paragraph_buffer.append(stripped)

    flush_paragraph(story, paragraph_buffer)
    if table_buffer:
        table = make_table(table_buffer)
        if table:
            story.append(table)

    return story


def draw_footer(canvas, doc) -> None:
    canvas.saveState()
    canvas.setStrokeColor(colors.HexColor("#E2E8F0"))
    canvas.setLineWidth(0.4)
    canvas.line(MARGIN, 1.2 * cm, PAGE_WIDTH - MARGIN, 1.2 * cm)
    canvas.setFont("Helvetica", 7)
    canvas.setFillColor(colors.HexColor("#64748B"))
    canvas.drawString(MARGIN, 0.75 * cm, "VehicleSalesFIAP - Tech Challenge FIAP/SOAT Fase 3")
    canvas.drawRightString(PAGE_WIDTH - MARGIN, 0.75 * cm, f"Pagina {doc.page}")
    canvas.restoreState()


def main() -> None:
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    story = parse_markdown(SOURCE.read_text(encoding="utf-8"))
    doc = SimpleDocTemplate(
        str(OUTPUT),
        pagesize=A4,
        rightMargin=MARGIN,
        leftMargin=MARGIN,
        topMargin=1.6 * cm,
        bottomMargin=1.7 * cm,
        title="VehicleSalesFIAP - Entrega Final",
        author="Elienai Roberto Parronchi",
        subject="Tech Challenge FIAP/SOAT - Fase 3",
    )
    doc.build(story, onFirstPage=draw_footer, onLaterPages=draw_footer)
    print(OUTPUT)


if __name__ == "__main__":
    main()
