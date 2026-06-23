from __future__ import annotations

import html
import re
import shutil
import textwrap
from pathlib import Path

import fitz
import pdfplumber
from PIL import Image, ImageDraw
from pypdf import PdfReader
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfbase import pdfmetrics
from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    HRFlowable,
    KeepTogether,
    PageBreak,
    PageTemplate,
    Paragraph,
    Preformatted,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "docs" / "DOCUMENTACAO_TECNICA.md"
OUTPUT_DIR = ROOT / "output" / "pdf"
OUTPUT = OUTPUT_DIR / "Documentacao_UsuariosAPI.pdf"
RENDER_DIR = ROOT / "tmp" / "pdfs" / "rendered"
CONTACT_DIR = ROOT / "tmp" / "pdfs" / "contact-sheets"

PAGE_W, PAGE_H = A4
LEFT = 20 * mm
RIGHT = 20 * mm
TOP = 20 * mm
BOTTOM = 18 * mm
CONTENT_W = PAGE_W - LEFT - RIGHT

NAVY = colors.HexColor("#13213A")
BLUE = colors.HexColor("#3157D5")
LIGHT_BLUE = colors.HexColor("#EAF0FF")
GREEN = colors.HexColor("#1E7A4D")
LIGHT_GREEN = colors.HexColor("#EAF8F0")
TEXT = colors.HexColor("#202A3B")
MUTED = colors.HexColor("#687386")
LINE = colors.HexColor("#DDE3ED")
CODE_BG = colors.HexColor("#F4F6FA")
WHITE = colors.white


def register_fonts() -> tuple[str, str, str, str]:
    fonts = Path("C:/Windows/Fonts")
    candidates = {
        "normal": fonts / "arial.ttf",
        "bold": fonts / "arialbd.ttf",
        "italic": fonts / "ariali.ttf",
        "mono": fonts / "consola.ttf",
    }
    if all(path.exists() for path in candidates.values()):
        pdfmetrics.registerFont(TTFont("DocSans", str(candidates["normal"])))
        pdfmetrics.registerFont(TTFont("DocSans-Bold", str(candidates["bold"])))
        pdfmetrics.registerFont(TTFont("DocSans-Italic", str(candidates["italic"])))
        pdfmetrics.registerFont(TTFont("DocMono", str(candidates["mono"])))
        return "DocSans", "DocSans-Bold", "DocSans-Italic", "DocMono"
    return "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Courier"


FONT, FONT_BOLD, FONT_ITALIC, FONT_MONO = register_fonts()


def build_styles():
    base = getSampleStyleSheet()
    return {
        "body": ParagraphStyle(
            "Body",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=9.2,
            leading=13.2,
            textColor=TEXT,
            spaceAfter=6,
        ),
        "small": ParagraphStyle(
            "Small",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=7.7,
            leading=10.5,
            textColor=MUTED,
        ),
        "h2": ParagraphStyle(
            "H2",
            parent=base["Heading1"],
            fontName=FONT_BOLD,
            fontSize=16,
            leading=20,
            textColor=NAVY,
            spaceBefore=12,
            spaceAfter=8,
            keepWithNext=True,
        ),
        "h3": ParagraphStyle(
            "H3",
            parent=base["Heading2"],
            fontName=FONT_BOLD,
            fontSize=12,
            leading=15,
            textColor=BLUE,
            spaceBefore=9,
            spaceAfter=5,
            keepWithNext=True,
        ),
        "h4": ParagraphStyle(
            "H4",
            parent=base["Heading3"],
            fontName=FONT_BOLD,
            fontSize=10,
            leading=13,
            textColor=NAVY,
            spaceBefore=7,
            spaceAfter=4,
            keepWithNext=True,
        ),
        "bullet": ParagraphStyle(
            "Bullet",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=9,
            leading=12.3,
            textColor=TEXT,
            leftIndent=12,
            firstLineIndent=-8,
            spaceAfter=3,
        ),
        "code": ParagraphStyle(
            "Code",
            parent=base["Code"],
            fontName=FONT_MONO,
            fontSize=7.2,
            leading=9.5,
            textColor=colors.HexColor("#25314A"),
            leftIndent=8,
            rightIndent=8,
            borderColor=LINE,
            borderWidth=0.7,
            borderPadding=8,
            backColor=CODE_BG,
            spaceBefore=4,
            spaceAfter=8,
        ),
        "table": ParagraphStyle(
            "TableCell",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=7.3,
            leading=9.4,
            textColor=TEXT,
        ),
        "table_header": ParagraphStyle(
            "TableHeader",
            parent=base["BodyText"],
            fontName=FONT_BOLD,
            fontSize=7.4,
            leading=9.5,
            textColor=WHITE,
        ),
        "cover_title": ParagraphStyle(
            "CoverTitle",
            parent=base["Title"],
            fontName=FONT_BOLD,
            fontSize=28,
            leading=33,
            textColor=WHITE,
            alignment=TA_LEFT,
        ),
        "cover_subtitle": ParagraphStyle(
            "CoverSubtitle",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=13,
            leading=18,
            textColor=colors.HexColor("#DDE6FF"),
        ),
        "cover_meta": ParagraphStyle(
            "CoverMeta",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=9.5,
            leading=14,
            textColor=WHITE,
        ),
        "toc": ParagraphStyle(
            "Toc",
            parent=base["BodyText"],
            fontName=FONT,
            fontSize=9.5,
            leading=15,
            textColor=TEXT,
            leftIndent=8,
        ),
    }


STYLES = build_styles()


class DocumentationDocTemplate(BaseDocTemplate):
    def __init__(self, filename: str):
        super().__init__(
            filename,
            pagesize=A4,
            leftMargin=LEFT,
            rightMargin=RIGHT,
            topMargin=TOP,
            bottomMargin=BOTTOM,
            title="Documentação Técnica e Guia de Aprovação - UsuariosAPI",
            author="UsuariosAPI",
            subject="Arquitetura, execução, API, segurança, testes e aprovação",
        )
        cover_frame = Frame(0, 0, PAGE_W, PAGE_H, id="cover", showBoundary=0)
        body_frame = Frame(LEFT, BOTTOM, CONTENT_W, PAGE_H - TOP - BOTTOM, id="body", showBoundary=0)
        self.addPageTemplates(
            [
                PageTemplate(id="Cover", frames=[cover_frame], onPage=draw_cover_background),
                PageTemplate(id="Body", frames=[body_frame], onPage=draw_header_footer),
            ]
        )

    def afterFlowable(self, flowable):
        if isinstance(flowable, PageBreak) and self.page == 1:
            self.handle_nextPageTemplate("Body")


def draw_cover_background(canvas, doc):
    canvas.saveState()
    canvas.setFillColor(NAVY)
    canvas.rect(0, 0, PAGE_W, PAGE_H, fill=1, stroke=0)
    canvas.setFillColor(BLUE)
    canvas.circle(PAGE_W - 22 * mm, PAGE_H - 25 * mm, 42 * mm, fill=1, stroke=0)
    canvas.setFillColor(colors.HexColor("#2448B8"))
    canvas.circle(PAGE_W - 4 * mm, PAGE_H - 6 * mm, 28 * mm, fill=1, stroke=0)
    canvas.restoreState()


def draw_header_footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(LINE)
    canvas.setLineWidth(0.5)
    canvas.line(LEFT, PAGE_H - 13 * mm, PAGE_W - RIGHT, PAGE_H - 13 * mm)
    canvas.setFont(FONT_BOLD, 7.5)
    canvas.setFillColor(NAVY)
    canvas.drawString(LEFT, PAGE_H - 10 * mm, "UsuariosAPI")
    canvas.setFont(FONT, 7.5)
    canvas.setFillColor(MUTED)
    canvas.drawRightString(PAGE_W - RIGHT, PAGE_H - 10 * mm, "Documentação Técnica e Guia de Aprovação")
    canvas.line(LEFT, 12 * mm, PAGE_W - RIGHT, 12 * mm)
    canvas.setFont(FONT, 7.2)
    canvas.drawString(LEFT, 8 * mm, "Revisão 22/06/2026")
    canvas.drawRightString(PAGE_W - RIGHT, 8 * mm, f"Página {doc.page}")
    canvas.restoreState()


def inline_markup(text: str) -> str:
    escaped = html.escape(text, quote=False)
    escaped = re.sub(
        r"\[([^\]]+)\]\(([^)]+)\)",
        r'<link href="\2" color="#3157D5">\1</link>',
        escaped,
    )
    escaped = re.sub(
        r"`([^`]+)`",
        rf'<font name="{FONT_MONO}" color="#2448B8">\1</font>',
        escaped,
    )
    escaped = re.sub(r"\*\*([^*]+)\*\*", r"<b>\1</b>", escaped)
    return escaped


def wrap_code(text: str, width: int = 94) -> str:
    output = []
    for line in text.splitlines() or [""]:
        if len(line) <= width:
            output.append(line)
            continue
        indent = re.match(r"\s*", line).group(0)
        output.extend(
            textwrap.wrap(
                line,
                width=width,
                subsequent_indent=indent + "  ",
                replace_whitespace=False,
                drop_whitespace=False,
                break_long_words=True,
                break_on_hyphens=False,
            )
        )
    return "\n".join(output)


def table_widths(column_count: int):
    if column_count == 2:
        ratios = [0.30, 0.70]
    elif column_count == 3:
        ratios = [0.24, 0.22, 0.54]
    elif column_count == 4:
        ratios = [0.18, 0.27, 0.18, 0.37]
    else:
        ratios = [1 / column_count] * column_count
    return [CONTENT_W * ratio for ratio in ratios]


def make_table(rows: list[list[str]]):
    max_cols = max(len(row) for row in rows)
    normalized = [row + [""] * (max_cols - len(row)) for row in rows]
    data = []
    for row_index, row in enumerate(normalized):
        style = STYLES["table_header"] if row_index == 0 else STYLES["table"]
        data.append([Paragraph(inline_markup(cell.strip()), style) for cell in row])
    table = Table(data, colWidths=table_widths(max_cols), repeatRows=1, hAlign="LEFT")
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, 0), NAVY),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("GRID", (0, 0), (-1, -1), 0.45, LINE),
                ("ROWBACKGROUNDS", (0, 1), (-1, -1), [WHITE, colors.HexColor("#F8FAFD")]),
                ("LEFTPADDING", (0, 0), (-1, -1), 5),
                ("RIGHTPADDING", (0, 0), (-1, -1), 5),
                ("TOPPADDING", (0, 0), (-1, -1), 4),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
            ]
        )
    )
    return table


def parse_markdown(markdown: str):
    lines = markdown.splitlines()
    story = []
    index = 0
    in_code = False
    code_lines = []
    paragraph_lines = []

    def flush_paragraph():
        nonlocal paragraph_lines
        if paragraph_lines:
            text = " ".join(line.strip() for line in paragraph_lines)
            story.append(Paragraph(inline_markup(text), STYLES["body"]))
            paragraph_lines = []

    while index < len(lines):
        line = lines[index]

        if line.startswith("```"):
            flush_paragraph()
            if in_code:
                story.append(Preformatted(wrap_code("\n".join(code_lines)), STYLES["code"]))
                code_lines = []
                in_code = False
            else:
                in_code = True
            index += 1
            continue

        if in_code:
            code_lines.append(line)
            index += 1
            continue

        if line.startswith("|") and index + 1 < len(lines) and re.match(r"^\|?[\s:|-]+\|", lines[index + 1]):
            flush_paragraph()
            table_rows = []
            while index < len(lines) and lines[index].startswith("|"):
                raw = lines[index].strip().strip("|")
                cells = [cell.strip() for cell in raw.split("|")]
                if not all(re.fullmatch(r":?-{3,}:?", cell.replace(" ", "")) for cell in cells):
                    table_rows.append(cells)
                index += 1
            if table_rows:
                story.append(make_table(table_rows))
                story.append(Spacer(1, 7))
            continue

        if line.startswith("#### "):
            flush_paragraph()
            story.append(Paragraph(inline_markup(line[5:]), STYLES["h4"]))
        elif line.startswith("### "):
            flush_paragraph()
            story.append(Paragraph(inline_markup(line[4:]), STYLES["h3"]))
        elif line.startswith("## "):
            flush_paragraph()
            story.append(Spacer(1, 3))
            story.append(Paragraph(inline_markup(line[3:]), STYLES["h2"]))
            story.append(HRFlowable(width="100%", thickness=1.2, color=BLUE, spaceAfter=6))
        elif re.match(r"^\s*- \[[ xX]\] ", line):
            flush_paragraph()
            checked = "[x]" if "[x]" in line.lower() else "[ ]"
            content = re.sub(r"^\s*- \[[ xX]\] ", "", line)
            story.append(Paragraph(f"- {checked} {inline_markup(content)}", STYLES["bullet"]))
        elif re.match(r"^\s*- ", line):
            flush_paragraph()
            content = re.sub(r"^\s*- ", "", line)
            story.append(Paragraph(f"- {inline_markup(content)}", STYLES["bullet"]))
        elif re.match(r"^\s*\d+\. ", line):
            flush_paragraph()
            match = re.match(r"^\s*(\d+)\. (.*)", line)
            story.append(Paragraph(f"{match.group(1)}. {inline_markup(match.group(2))}", STYLES["bullet"]))
        elif not line.strip():
            flush_paragraph()
        elif line.startswith("# "):
            flush_paragraph()
        elif re.match(r"^(Versão|Data da revisão|Tecnologia principal|Banco de dados|Situação):", line):
            flush_paragraph()
        else:
            paragraph_lines.append(line)

        index += 1

    flush_paragraph()
    return story


def cover_story():
    meta_table = Table(
        [
            [Paragraph("VERSÃO", STYLES["small"]), Paragraph("1.0", STYLES["cover_meta"])],
            [Paragraph("REVISÃO", STYLES["small"]), Paragraph("22/06/2026", STYLES["cover_meta"])],
            [Paragraph("STACK", STYLES["small"]), Paragraph(".NET 8 + SQL Server + Docker", STYLES["cover_meta"])],
            [Paragraph("STATUS", STYLES["small"]), Paragraph("Pronto para análise e GitHub", STYLES["cover_meta"])],
        ],
        colWidths=[30 * mm, 88 * mm],
    )
    meta_table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), colors.HexColor("#1B2D4F")),
                ("BOX", (0, 0), (-1, -1), 0.5, colors.HexColor("#47618F")),
                ("INNERGRID", (0, 0), (-1, -1), 0.3, colors.HexColor("#47618F")),
                ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                ("LEFTPADDING", (0, 0), (-1, -1), 8),
                ("RIGHTPADDING", (0, 0), (-1, -1), 8),
                ("TOPPADDING", (0, 0), (-1, -1), 7),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
            ]
        )
    )
    return [
        Spacer(1, 54 * mm),
        Paragraph("UsuariosAPI", STYLES["cover_title"]),
        Spacer(1, 4 * mm),
        Paragraph("Documentação Técnica<br/>e Guia de Aprovação", STYLES["cover_title"]),
        Spacer(1, 9 * mm),
        Paragraph(
            "Arquitetura, execução passo a passo, contrato REST, banco de dados, "
            "segurança, testes, riscos e publicação no GitHub.",
            STYLES["cover_subtitle"],
        ),
        Spacer(1, 16 * mm),
        meta_table,
        PageBreak(),
    ]


def summary_story():
    toc_items = [
        "1-6  Escopo, arquitetura, camadas e tecnologias",
        "7-10  Banco de dados e execução",
        "11-14  API, validações, erros e Swagger",
        "15-18  Segurança, Docker, testes e avaliação",
        "19-22  Problemas, aprovação, GitHub e parecer final",
    ]
    validation_table = Table(
        [
            [Paragraph("BUILD", STYLES["table_header"]), Paragraph("TESTES", STYLES["table_header"]), Paragraph("NUGET", STYLES["table_header"]), Paragraph("DOCKER", STYLES["table_header"])],
            [Paragraph("Aprovado", STYLES["table"]), Paragraph("29 / 29", STYLES["table"]), Paragraph("0 vulneráveis", STYLES["table"]), Paragraph("Healthy", STYLES["table"])],
        ],
        colWidths=[CONTENT_W / 4] * 4,
    )
    validation_table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, 0), NAVY),
                ("BACKGROUND", (0, 1), (-1, 1), LIGHT_GREEN),
                ("GRID", (0, 0), (-1, -1), 0.5, LINE),
                ("ALIGN", (0, 0), (-1, -1), "CENTER"),
                ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                ("TOPPADDING", (0, 0), (-1, -1), 8),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
            ]
        )
    )
    story = [
        Paragraph("Leitura rápida", STYLES["h2"]),
        HRFlowable(width="100%", thickness=1.2, color=BLUE, spaceAfter=8),
        Paragraph(
            "O projeto está funcional e apto para avaliação local e publicação do código no GitHub. "
            "A exposição da aplicação à internet ainda depende de autenticação, rate limiting, HTTPS, "
            "privilégio mínimo do SQL Server e logs de auditoria.",
            STYLES["body"],
        ),
        Spacer(1, 5),
        validation_table,
        Spacer(1, 15),
        Paragraph("Mapa do documento", STYLES["h3"]),
    ]
    story.extend(Paragraph(item, STYLES["toc"]) for item in toc_items)
    story.extend(
        [
            Spacer(1, 14),
            KeepTogether(
                [
                    Table(
                        [[Paragraph("DECISÃO RECOMENDADA", STYLES["table_header"])]],
                        colWidths=[CONTENT_W],
                        style=TableStyle(
                            [
                                ("BACKGROUND", (0, 0), (-1, -1), BLUE),
                                ("LEFTPADDING", (0, 0), (-1, -1), 8),
                                ("TOPPADDING", (0, 0), (-1, -1), 7),
                                ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
                            ]
                        ),
                    ),
                    Table(
                        [[Paragraph("Aprovar para demonstração e GitHub. Condicionar o deploy público ao pacote de segurança pendente.", STYLES["body"])]],
                        colWidths=[CONTENT_W],
                        style=TableStyle(
                            [
                                ("BACKGROUND", (0, 0), (-1, -1), LIGHT_BLUE),
                                ("BOX", (0, 0), (-1, -1), 0.6, BLUE),
                                ("LEFTPADDING", (0, 0), (-1, -1), 9),
                                ("RIGHTPADDING", (0, 0), (-1, -1), 9),
                                ("TOPPADDING", (0, 0), (-1, -1), 9),
                                ("BOTTOMPADDING", (0, 0), (-1, -1), 9),
                            ]
                        ),
                    ),
                ]
            ),
            PageBreak(),
        ]
    )
    return story


def render_and_validate():
    if RENDER_DIR.exists():
        shutil.rmtree(RENDER_DIR)
    if CONTACT_DIR.exists():
        shutil.rmtree(CONTACT_DIR)
    RENDER_DIR.mkdir(parents=True, exist_ok=True)
    CONTACT_DIR.mkdir(parents=True, exist_ok=True)

    pdf = fitz.open(OUTPUT)
    for page_index, page in enumerate(pdf):
        pix = page.get_pixmap(matrix=fitz.Matrix(1.35, 1.35), alpha=False)
        pix.save(RENDER_DIR / f"page-{page_index + 1:02d}.png")

    pages = [Image.open(path).convert("RGB") for path in sorted(RENDER_DIR.glob("page-*.png"))]
    thumb_w = 250
    thumb_h = int(thumb_w * PAGE_H / PAGE_W)
    per_sheet = 6
    for sheet_index in range(0, len(pages), per_sheet):
        group = pages[sheet_index : sheet_index + per_sheet]
        sheet = Image.new("RGB", (thumb_w * 3 + 40, thumb_h * 2 + 70), "#D8DDE7")
        draw = ImageDraw.Draw(sheet)
        for local_index, page_image in enumerate(group):
            page_number = sheet_index + local_index + 1
            thumb = page_image.copy()
            thumb.thumbnail((thumb_w, thumb_h))
            x = 10 + (local_index % 3) * (thumb_w + 10)
            y = 28 + (local_index // 3) * (thumb_h + 30)
            sheet.paste(thumb, (x, y))
            draw.text((x, 8 + (local_index // 3) * (thumb_h + 30)), f"Página {page_number}", fill="#13213A")
        sheet.save(CONTACT_DIR / f"contact-{sheet_index // per_sheet + 1:02d}.png")

    reader = PdfReader(str(OUTPUT))
    with pdfplumber.open(OUTPUT) as checked:
        empty_pages = [index + 1 for index, page in enumerate(checked.pages) if not (page.extract_text() or "").strip()]
    if empty_pages:
        raise RuntimeError(f"Páginas sem texto detectadas: {empty_pages}")

    print(f"PDF={OUTPUT}")
    print(f"PAGES={len(reader.pages)}")
    print(f"SIZE_BYTES={OUTPUT.stat().st_size}")
    print(f"RENDERED={len(pages)}")
    print(f"CONTACT_SHEETS={len(list(CONTACT_DIR.glob('contact-*.png')))}")


def main():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    markdown = SOURCE.read_text(encoding="utf-8")
    first_section = markdown.find("## 1.")
    if first_section < 0:
        raise RuntimeError("A primeira seção numerada não foi encontrada.")

    story = cover_story()
    story.extend(summary_story())
    story.extend(parse_markdown(markdown[first_section:]))

    doc = DocumentationDocTemplate(str(OUTPUT))
    doc.build(story)
    render_and_validate()


if __name__ == "__main__":
    main()
