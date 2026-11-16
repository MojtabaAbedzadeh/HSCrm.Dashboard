/* ==========================================================================
   HSCrm Invoice Print Engine & Template Designer (Powered by Print.js)
   ========================================================================== */
(function (window, document) {
    'use strict';

    const DEFAULT_TEMPLATE = {
        theme: 'classic',           // classic | modern | corporate
        accentColor: '#1e40af',
        paperSize: 'A4 portrait',   // A4 portrait | A4 landscape | A5 portrait | A5 landscape
        fontSize: 12,               // 11 | 12 | 13 | 14
        salesTitle: 'صورت‌حساب فروش کالا و خدمات',
        purchaseTitle: 'فاکتور خرید کالا و خدمات',
        showLogo: true,
        logoPosition: 'right',      // right | center | left
        showCompanyMeta: true,
        showRowNumber: true,
        showUnit: true,
        showDiscount: true,
        showTax: true,
        showPaymentSummary: true,
        showAmountInWords: true,
        showNotes: true,
        showFooterText: true,
        showSignatures: true,
        sig1Label: 'مهر و امضای صادرکننده',
        sig2Label: 'تأیید واحد مالی و حسابداری',
        sig3Label: 'مهر و امضای طرف حساب'
    };

    const LOCAL_TEMPLATE_KEY = 'hscrm_invoice_print_tpl';
    let lastPreviewContext = null;

    function esc(v) {
        return String(v ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function fmtNum(val) {
        const n = Number(val || 0);
        return isNaN(n) ? '0' : n.toLocaleString('fa-IR');
    }

    function toPersianDate(dateVal, fallbackShamsi) {
        if (fallbackShamsi) return fallbackShamsi;
        if (!dateVal) return '-';
        try {
            const d = new Date(dateVal);
            if (!isNaN(d.getTime())) {
                return new Intl.DateTimeFormat('fa-IR-u-ca-persian', {
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit'
                }).format(d);
            }
        } catch { }
        return String(dateVal).substring(0, 10);
    }

    function numberToPersianWords(num) {
        const n = Math.floor(Math.abs(Number(num || 0)));
        if (!isFinite(n) || n === 0) return 'صفر';

        const ones = ['', 'یک', 'دو', 'سه', 'چهار', 'پنج', 'شش', 'هفت', 'هشت', 'نه'];
        const teens = ['ده', 'یازده', 'دوازده', 'سیزده', 'چهارده', 'پانزده', 'شانزده', 'هفده', 'هجده', 'نوزده'];
        const tens = ['', '', 'بیست', 'سی', 'چهل', 'پنجاه', 'شصت', 'هفتاد', 'هشتاد', 'نود'];
        const hundreds = ['', 'صد', 'دویست', 'سیصد', 'چهارصد', 'پانصد', 'ششصد', 'هفتصد', 'هشتصد', 'نهصد'];
        const scales = ['', 'هزار', 'میلیون', 'میلیارد', 'هزار میلیارد'];

        function threeDigits(x) {
            const parts = [];
            const h = Math.floor(x / 100);
            const r = x % 100;
            if (h > 0) parts.push(hundreds[h]);
            if (r >= 10 && r < 20) {
                parts.push(teens[r - 10]);
            } else {
                const t = Math.floor(r / 10);
                const o = r % 10;
                if (t > 0) parts.push(tens[t]);
                if (o > 0) parts.push(ones[o]);
            }
            return parts.join(' و ');
        }

        const groups = [];
        let temp = n;
        let idx = 0;
        while (temp > 0) {
            const chunk = temp % 1000;
            if (chunk > 0) {
                const words = threeDigits(chunk);
                groups.unshift(scales[idx] ? `${words} ${scales[idx]}` : words);
            }
            temp = Math.floor(temp / 1000);
            idx++;
        }
        return groups.join(' و ');
    }

    function mergeTemplate(raw) {
        let parsed = {};
        if (typeof raw === 'string' && raw.trim()) {
            try { parsed = JSON.parse(raw); } catch { parsed = {}; }
        } else if (raw && typeof raw === 'object') {
            parsed = raw;
        }
        return Object.assign({}, DEFAULT_TEMPLATE, parsed || {});
    }

    function getLocalTemplate() {
        try {
            const cached = window.__hscrmBranding && window.__hscrmBranding.invoicePrintTemplateJson;
            if (cached) return mergeTemplate(cached);
            const local = localStorage.getItem(LOCAL_TEMPLATE_KEY);
            if (local) return mergeTemplate(local);
        } catch { }
        return mergeTemplate({});
    }

    function saveLocalTemplate(tpl) {
        try {
            const merged = mergeTemplate(tpl);
            localStorage.setItem(LOCAL_TEMPLATE_KEY, JSON.stringify(merged));
            if (window.__hscrmBranding) {
                window.__hscrmBranding.invoicePrintTemplateJson = JSON.stringify(merged);
            }
            for (let i = 0; i < sessionStorage.length; i++) {
                const k = sessionStorage.key(i);
                if (k && k.startsWith('hscrm_tenant_branding_')) {
                    const obj = JSON.parse(sessionStorage.getItem(k) || '{}');
                    obj.invoicePrintTemplateJson = JSON.stringify(merged);
                    sessionStorage.setItem(k, JSON.stringify(obj));
                }
            }
        } catch { }
    }

    function resolveLogo(logoUrl, apiBase) {
        if (!logoUrl) return '';
        if (logoUrl.startsWith('http://') || logoUrl.startsWith('https://') || logoUrl.startsWith('data:')) {
            return logoUrl;
        }
        const origin = String(apiBase || '').replace(/\/api\/?$/i, '').replace(/\/+$/, '');
        return origin + (logoUrl.startsWith('/') ? logoUrl : '/' + logoUrl);
    }

    function buildInvoiceCss(tpl, forPrint) {
        const accent = /^#[0-9a-fA-F]{3,8}$/.test(tpl.accentColor || '') ? tpl.accentColor : DEFAULT_TEMPLATE.accentColor;
        const fontSize = Math.min(Math.max(Number(tpl.fontSize) || 12, 10), 16);
        const pageSize = tpl.paperSize || 'A4 portrait';
        const isCorporate = tpl.theme === 'corporate';
        const isModern = tpl.theme === 'modern';

        return `
            ${forPrint ? `@page { size: ${pageSize}; margin: 10mm; }` : ''}
            .hsp-sheet {
                font-family: 'IRANSans', 'Vazir', Tahoma, Arial, sans-serif;
                direction: rtl;
                text-align: right;
                color: #1f2937;
                font-size: ${fontSize}px;
                line-height: 1.55;
                background: #ffffff;
                border: ${isModern ? '1px solid #e5e7eb' : `2px solid ${accent}`};
                border-radius: ${isModern ? '10px' : '4px'};
                padding: 18px 20px;
                box-sizing: border-box;
                width: 100%;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .hsp-sheet * { box-sizing: border-box; }
            .hsp-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                gap: 14px;
                padding: ${isCorporate ? '14px 16px' : '0 0 14px 0'};
                margin-bottom: 14px;
                border-bottom: ${isCorporate ? '0' : `2px solid ${accent}`};
                background: ${isCorporate ? accent : 'transparent'};
                color: ${isCorporate ? '#ffffff' : '#111827'};
                border-radius: ${isCorporate ? '6px' : '0'};
                flex-wrap: wrap;
            }
            .hsp-brand-block {
                display: flex;
                align-items: center;
                gap: 12px;
                flex: 1 1 auto;
                justify-content: ${tpl.logoPosition === 'center' ? 'center' : tpl.logoPosition === 'left' ? 'flex-end' : 'flex-start'};
            }
            .hsp-logo {
                max-height: 64px;
                max-width: 110px;
                object-fit: contain;
                background: #ffffff;
                padding: 4px;
                border-radius: 6px;
                border: 1px solid #e5e7eb;
            }
            .hsp-company-name {
                font-size: ${fontSize + 5}px;
                font-weight: 800;
                margin: 0 0 2px 0;
                color: ${isCorporate ? '#ffffff' : accent};
            }
            .hsp-doc-title {
                font-size: ${fontSize + 2}px;
                font-weight: 700;
                margin: 0;
            }
            .hsp-company-meta {
                font-size: ${fontSize - 1}px;
                opacity: 0.88;
                margin-top: 3px;
            }
            .hsp-doc-meta {
                min-width: 195px;
                background: ${isCorporate ? 'rgba(255,255,255,0.14)' : '#f8fafc'};
                border: 1px solid ${isCorporate ? 'rgba(255,255,255,0.28)' : '#dbe3ef'};
                border-radius: 6px;
                padding: 8px 12px;
                font-size: ${fontSize - 1}px;
            }
            .hsp-doc-meta div {
                display: flex;
                justify-content: space-between;
                gap: 8px;
                padding: 2px 0;
            }
            .hsp-party-box {
                display: grid;
                grid-template-columns: repeat(3, minmax(0, 1fr));
                gap: 8px 14px;
                background: #f8fafc;
                border: 1px solid #dbe3ef;
                border-radius: 6px;
                padding: 10px 14px;
                margin-bottom: 14px;
                font-size: ${fontSize - 0.5}px;
            }
            .hsp-party-full { grid-column: 1 / -1; }
            .hsp-table {
                width: 100%;
                border-collapse: collapse;
                margin-bottom: 14px;
                font-size: ${fontSize - 0.5}px;
            }
            .hsp-table th {
                background: ${isModern ? '#f1f5f9' : accent};
                color: ${isModern ? '#0f172a' : '#ffffff'};
                border: 1px solid ${isModern ? '#cbd5e1' : accent};
                padding: 7px 6px;
                font-weight: 700;
                text-align: center;
                white-space: nowrap;
            }
            .hsp-table td {
                border: 1px solid #cbd5e1;
                padding: 6px 7px;
                text-align: center;
                vertical-align: middle;
            }
            .hsp-table tbody tr:nth-child(even) { background: #f9fafb; }
            .hsp-table td.hsp-text-start { text-align: right; font-weight: 600; }
            .hsp-mono { font-family: Tahoma, Arial, sans-serif; direction: ltr; unicode-bidi: embed; }
            .hsp-totals-wrap {
                display: flex;
                justify-content: space-between;
                align-items: flex-start;
                gap: 14px;
                flex-wrap: wrap;
                margin-bottom: 12px;
            }
            .hsp-words-box {
                flex: 1 1 300px;
                background: #f8fafc;
                border: 1px dashed #cbd5e1;
                border-radius: 6px;
                padding: 9px 12px;
                font-size: ${fontSize - 0.5}px;
            }
            .hsp-summary-table {
                min-width: 260px;
                border-collapse: collapse;
                font-size: ${fontSize - 0.5}px;
            }
            .hsp-summary-table td {
                border: 1px solid #cbd5e1;
                padding: 5px 10px;
            }
            .hsp-summary-total td {
                background: ${accent};
                color: #ffffff;
                font-weight: 800;
                border-color: ${accent};
            }
            .hsp-footer-note {
                background: #fffbeb;
                border: 1px solid #fde68a;
                border-radius: 6px;
                padding: 8px 12px;
                font-size: ${fontSize - 1}px;
                color: #78350f;
                margin-bottom: 14px;
            }
            .hsp-signatures {
                display: grid;
                grid-template-columns: repeat(3, 1fr);
                gap: 18px;
                margin-top: 26px;
                text-align: center;
                font-size: ${fontSize - 1}px;
            }
            .hsp-sig-box {
                border-top: 1px solid #334155;
                padding-top: 6px;
                min-height: 44px;
                font-weight: 600;
                color: #334155;
            }
        `;
    }

    function renderInvoiceBody(data, template) {
        const tpl = mergeTemplate(template);
        const isPurchase = data.invoiceKind === 'purchase';
        const docTitle = isPurchase ? tpl.purchaseTitle : tpl.salesTitle;
        const c = data.company || {};
        const currencyLabel = (c.currency === 'IRT') ? 'تومان' : 'ریال';

        const metaBits = [];
        if (c.taxId) metaBits.push(`شناسه/کد اقتصادی: ${esc(c.taxId)}`);
        if (c.companyPhone) metaBits.push(`تلفن: ${esc(c.companyPhone)}`);
        if (c.companyEmail) metaBits.push(`ایمیل: ${esc(c.companyEmail)}`);
        if (c.address) metaBits.push(`نشانی: ${esc(c.address)}`);

        const items = Array.isArray(data.items) ? data.items : [];
        let colCount = 4; // title, qty, unitPrice, lineTotal
        if (tpl.showRowNumber) colCount++;
        if (tpl.showUnit) colCount++;
        if (tpl.showDiscount) colCount++;
        if (tpl.showTax) colCount++;

        const rowsHtml = items.length === 0
            ? `<tr><td colspan="${colCount}" style="padding:14px;color:#64748b;">ردیفی برای نمایش ثبت نشده است.</td></tr>`
            : items.map((it, idx) => {
                const qty = Number(it.quantity ?? 1);
                const price = Number(it.unitPrice ?? 0);
                const disc = Number(it.discount ?? 0);
                const tax = Number(it.tax ?? 0);
                const lineTotal = Number(it.lineTotal ?? ((qty * price) - disc + tax));
                return `<tr>
                    ${tpl.showRowNumber ? `<td>${idx + 1}</td>` : ''}
                    <td class="hsp-text-start">${esc(it.title || 'کالا / خدمات')}</td>
                    ${tpl.showUnit ? `<td>${esc(it.unit || 'عدد')}</td>` : ''}
                    <td class="hsp-mono">${fmtNum(qty)}</td>
                    <td class="hsp-mono">${fmtNum(price)}</td>
                    ${tpl.showDiscount ? `<td class="hsp-mono">${fmtNum(disc)}</td>` : ''}
                    ${tpl.showTax ? `<td class="hsp-mono">${fmtNum(tax)}</td>` : ''}
                    <td class="hsp-mono" style="font-weight:700;">${fmtNum(lineTotal)}</td>
                </tr>`;
            }).join('');

        const totalVal = Number(data.total || 0);
        const paidVal = Number(data.paidAmount || 0);
        const remVal = Math.max(0, Number(data.remainingAmount ?? (totalVal - paidVal)));
        const logoImg = (tpl.showLogo && c.logoSrc)
            ? `<img class="hsp-logo" src="${esc(c.logoSrc)}" alt="Logo" />`
            : '';

        return `
        <div class="hsp-sheet">
            <div class="hsp-header">
                <div class="hsp-brand-block">
                    ${tpl.logoPosition !== 'left' ? logoImg : ''}
                    <div>
                        <div class="hsp-company-name">${esc(c.companyTitle || 'شرکت')}</div>
                        <div class="hsp-doc-title">${esc(docTitle)}</div>
                        ${tpl.showCompanyMeta && metaBits.length > 0
                            ? `<div class="hsp-company-meta">${metaBits.join(' &nbsp;|&nbsp; ')}</div>`
                            : ''}
                    </div>
                    ${tpl.logoPosition === 'left' ? logoImg : ''}
                </div>
                <div class="hsp-doc-meta">
                    <div><span>شماره فاکتور:</span><strong class="hsp-mono">${esc(data.number || '-')}</strong></div>
                    <div><span>تاریخ صدور:</span><strong class="hsp-mono">${esc(data.dateShamsi || '-')}</strong></div>
                    <div><span>وضعیت:</span><strong>${esc(data.statusText || '-')}</strong></div>
                </div>
            </div>

            <div class="hsp-party-box">
                <div><strong>${isPurchase ? 'تأمین‌کننده' : 'خریدار'}:</strong> ${esc(data.partyName || '-')}</div>
                <div><strong>${isPurchase ? 'انبار مقصد' : 'پروژه'}:</strong> ${esc(data.secondaryRef || '-')}</div>
                <div><strong>تلفن:</strong> <span class="hsp-mono">${esc(data.partyPhone || '-')}</span></div>
                ${data.partyAddress ? `<div class="hsp-party-full"><strong>نشانی طرف حساب:</strong> ${esc(data.partyAddress)}</div>` : ''}
            </div>

            <table class="hsp-table">
                <thead>
                    <tr>
                        ${tpl.showRowNumber ? '<th>ردیف</th>' : ''}
                        <th>شرح کالا / خدمات</th>
                        ${tpl.showUnit ? '<th>واحد</th>' : ''}
                        <th>تعداد</th>
                        <th>مبلغ واحد (${currencyLabel})</th>
                        ${tpl.showDiscount ? '<th>تخفیف</th>' : ''}
                        ${tpl.showTax ? '<th>مالیات</th>' : ''}
                        <th>جمع کل (${currencyLabel})</th>
                    </tr>
                </thead>
                <tbody>${rowsHtml}</tbody>
            </table>

            <div class="hsp-totals-wrap">
                <div style="flex:1 1 280px;display:flex;flex-direction:column;gap:8px;">
                    ${tpl.showAmountInWords
                        ? `<div class="hsp-words-box"><strong>مبلغ قابل پرداخت به حروف:</strong> ${esc(numberToPersianWords(totalVal))} ${currencyLabel}</div>`
                        : ''}
                    ${tpl.showNotes && data.notes
                        ? `<div class="hsp-words-box"><strong>توضیحات فاکتور:</strong> ${esc(data.notes)}</div>`
                        : ''}
                </div>

                <table class="hsp-summary-table">
                    ${data.subtotal > 0 && data.subtotal !== totalVal
                        ? `<tr><td>جمع اقلام:</td><td class="hsp-mono">${fmtNum(data.subtotal)} ${currencyLabel}</td></tr>`
                        : ''}
                    ${Number(data.discount || 0) > 0
                        ? `<tr><td>تخفیف کل:</td><td class="hsp-mono">${fmtNum(data.discount)} ${currencyLabel}</td></tr>`
                        : ''}
                    ${Number(data.tax || 0) > 0
                        ? `<tr><td>مالیات و عوارض:</td><td class="hsp-mono">${fmtNum(data.tax)} ${currencyLabel}</td></tr>`
                        : ''}
                    <tr class="hsp-summary-total">
                        <td>جمع کل قابل پرداخت:</td>
                        <td class="hsp-mono">${fmtNum(totalVal)} ${currencyLabel}</td>
                    </tr>
                    ${tpl.showPaymentSummary ? `
                        <tr><td>مبلغ پرداخت‌شده / تسویه:</td><td class="hsp-mono">${fmtNum(paidVal)} ${currencyLabel}</td></tr>
                        <tr><td>مانده حساب:</td><td class="hsp-mono" style="font-weight:700;color:${remVal > 0 ? '#dc2626' : '#15803d'};">${fmtNum(remVal)} ${currencyLabel}</td></tr>
                    ` : ''}
                </table>
            </div>

            ${tpl.showFooterText && c.footerText
                ? `<div class="hsp-footer-note"><strong>شرایط و توضیحات:</strong> ${esc(c.footerText)}</div>`
                : ''}

            ${tpl.showSignatures ? `
                <div class="hsp-signatures">
                    <div class="hsp-sig-box">${esc(tpl.sig1Label)}</div>
                    <div class="hsp-sig-box">${esc(tpl.sig2Label)}</div>
                    <div class="hsp-sig-box">${esc(tpl.sig3Label)}</div>
                </div>
            ` : ''}
        </div>`;
    }

    function printInvoiceData(data, template) {
        const tpl = mergeTemplate(template || getLocalTemplate());
        const bodyHtml = renderInvoiceBody(data, tpl);
        const css = buildInvoiceCss(tpl, true);

        if (typeof window.printJS === 'function') {
            window.printJS({
                printable: bodyHtml,
                type: 'raw-html',
                style: css,
                scanStyles: false,
                documentTitle: (data.invoiceKind === 'purchase' ? 'فاکتور خرید ' : 'فاکتور فروش ') + (data.number || '')
            });
            return;
        }

        // Isolated iframe fallback if printJS script failed to load
        const frame = document.createElement('iframe');
        frame.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;visibility:hidden;';
        document.body.appendChild(frame);
        const doc = frame.contentWindow.document;
        doc.open();
        doc.write(`<!DOCTYPE html><html lang="fa" dir="rtl"><head><meta charset="utf-8"><style>${css}</style></head><body>${bodyHtml}</body></html>`);
        doc.close();
        setTimeout(() => {
            frame.contentWindow.focus();
            frame.contentWindow.print();
            setTimeout(() => document.body.removeChild(frame), 1000);
        }, 250);
    }

    function printElementHtml(elementId, template, docTitle) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const tpl = mergeTemplate(template || getLocalTemplate());
        const css = buildInvoiceCss(tpl, true);
        if (typeof window.printJS === 'function') {
            window.printJS({
                printable: el.innerHTML,
                type: 'raw-html',
                style: css,
                scanStyles: false,
                documentTitle: docTitle || 'چاپ فاکتور'
            });
        }
    }

    async function fetchBrandingAndTemplate(apiBase, token) {
        const base = String(apiBase || '').replace(/\/+$/, '');
        if (!base || !token) return { company: {}, template: getLocalTemplate() };
        try {
            const res = await fetch(`${base}/Tenant/GetBranding`, {
                headers: { 'Authorization': 'Bearer ' + token },
                suppressGlobalAlert: true
            });
            if (!res.ok) return { company: {}, template: getLocalTemplate() };
            const json = await res.json();
            const b = json?.data || {};
            const tpl = b.invoicePrintTemplateJson ? mergeTemplate(b.invoicePrintTemplateJson) : getLocalTemplate();
            if (b.invoicePrintTemplateJson) saveLocalTemplate(tpl);
            return {
                company: {
                    companyTitle: b.tenantTitle || 'شرکت',
                    logoSrc: resolveLogo(b.logoUrl, base),
                    taxId: b.taxId || '',
                    companyPhone: b.companyPhone || '',
                    companyEmail: b.companyEmail || '',
                    address: b.address || '',
                    footerText: b.invoiceFooterText || '',
                    currency: b.currency || 'IRR'
                },
                template: tpl
            };
        } catch {
            return { company: {}, template: getLocalTemplate() };
        }
    }

    function ensurePreviewModal() {
        if (document.getElementById('hscrmUnifiedPrintModal')) return;
        const wrapper = document.createElement('div');
        wrapper.innerHTML = `
        <div class="modal fade" id="hscrmUnifiedPrintModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header bg-dark text-white d-flex justify-content-between align-items-center">
                        <h5 class="modal-title"><i class="mdi mdi-printer me-1"></i><span id="hscrmUpTitle">پیش‌نمایش چاپ رسمی فاکتور</span></h5>
                        <div class="d-flex align-items-center gap-2">
                            <button type="button" class="btn btn-sm btn-outline-light d-none" id="hscrmUpOpenDesignerBtn">
                                <i class="mdi mdi-palette-outline me-1"></i>طراحی قالب فاکتور
                            </button>
                            <button type="button" class="btn btn-sm btn-warning fw-bold" id="hscrmUpPrintBtn">
                                <i class="mdi mdi-printer me-1"></i>چاپ فاکتور
                            </button>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                    </div>
                    <div class="modal-body bg-light p-3 p-md-4">
                        <div id="hscrmUpPreviewHost" class="mx-auto" style="max-width: 920px;"></div>
                    </div>
                </div>
            </div>
        </div>`;
        document.body.appendChild(wrapper.firstElementChild);

        document.getElementById('hscrmUpPrintBtn').addEventListener('click', function () {
            if (!lastPreviewContext) return;
            printInvoiceData(lastPreviewContext.data, lastPreviewContext.template);
        });
    }

    async function openInvoicePrintModal(opts) {
        ensurePreviewModal();
        const type = opts.type === 'purchase' ? 'purchase' : 'sales';
        const apiBase = String(opts.apiBase || '').replace(/\/+$/, '');
        const token = opts.token || '';
        const authHeaders = { 'Authorization': 'Bearer ' + token };
        const host = document.getElementById('hscrmUpPreviewHost');
        const titleEl = document.getElementById('hscrmUpTitle');
        const designBtn = document.getElementById('hscrmUpOpenDesignerBtn');

        titleEl.textContent = type === 'purchase' ? 'پیش‌نمایش و چاپ فاکتور خرید' : 'پیش‌نمایش و چاپ رسمی فاکتور فروش';
        host.innerHTML = '<div class="text-center py-5 text-muted">در حال دریافت اطلاعات فاکتور و قالب چاپ...</div>';

        if (opts.canDesign) {
            designBtn.classList.remove('d-none');
            designBtn.onclick = function () {
                openDesignerModal({
                    apiBase,
                    token,
                    canSaveServer: true,
                    onSaved: function (newTpl) {
                        if (lastPreviewContext) {
                            lastPreviewContext.template = newTpl;
                            renderIntoHost(host, lastPreviewContext.data, newTpl);
                        }
                    }
                });
            };
        } else {
            designBtn.classList.add('d-none');
        }

        const mdl = bootstrap.Modal.getOrCreateInstance(document.getElementById('hscrmUnifiedPrintModal'));
        mdl.show();

        const branding = await fetchBrandingAndTemplate(apiBase, token);
        const fb = opts.fallbackSummary || {};

        let inv = {};
        try {
            const url = type === 'purchase'
                ? `${apiBase}/PurchaseInvoice/GetById?InvoiceId=${opts.invoiceId}`
                : `${apiBase}/SalesInvoice/GetById?invoiceId=${opts.invoiceId}`;
            const r = await fetch(url, { headers: authHeaders });
            const j = await r.json();
            inv = j?.data || j || {};
        } catch { }

        let paidAmount = Number(inv.paidAmount ?? fb.paidAmount ?? 0);
        let remainingAmount = inv.remainingAmount ?? fb.remainingAmount;

        if (type === 'sales' && remainingAmount === undefined) {
            try {
                const sr = await fetch(`${apiBase}/SalesInvoice/SettlementSummaries`, { headers: authHeaders, suppressGlobalAlert: true });
                const sj = await sr.json();
                const row = (sj?.data || []).find(s => Number(s.salesInvoiceId ?? s.referenceId) === Number(opts.invoiceId));
                if (row) {
                    paidAmount = Number(row.cashPaidAmount || 0) + Number(row.clearedChequesAmount || 0);
                    remainingAmount = Number(row.remainingAmount || 0);
                }
            } catch { }
        }

        const rawItems = inv.items || inv.salesInvoiceItems || inv.purchaseInvoiceItems || [];
        const mappedItems = Array.isArray(rawItems) ? rawItems.map(it => {
            const qty = Number(it.quantity ?? it.count ?? 1);
            const unitPrice = Number(
                type === 'purchase'
                    ? (it.unitPricePurchase || it.unitPrice || it.price || 0)
                    : (it.unitPriceSell ?? it.unitPrice ?? it.price ?? 0)
            );
            const discount = Number(it.discount ?? 0);
            const tax = Number(it.tax ?? it.vatAmount ?? 0);
            const lineTotal = Number(it.lineTotal ?? it.totalPrice ?? it.rowSum ?? ((qty * unitPrice) - discount + tax));
            return {
                title: it.product?.productTitle || it.productTitle || it.productName || ('کالای #' + (it.productId || '')),
                unit: it.product?.productUnit || it.unit || 'عدد',
                quantity: qty,
                unitPrice,
                discount,
                tax,
                lineTotal
            };
        }) : [];

        const totalVal = Number(inv.total ?? fb.total ?? 0);
        if (remainingAmount === undefined) {
            remainingAmount = Math.max(0, totalVal - paidAmount);
        }

        const statusCode = Number(inv.status ?? fb.status ?? 0);
        const statusText = type === 'purchase'
            ? (statusCode === 1 ? 'نهایی' : statusCode === 2 ? 'ابطال' : statusCode === 3 ? 'رد شده' : 'پیش‌نویس')
            : (statusCode === 1 ? 'فاکتور نهایی' : statusCode === 2 ? 'ابطال' : statusCode === 3 ? 'رد شده' : 'پیش‌فاکتور');

        const printData = {
            invoiceKind: type,
            number: inv.number || fb.number || String(opts.invoiceId),
            dateShamsi: fb.dateShamsi || toPersianDate(inv.issueDate, inv.issueDateShamsi),
            statusText,
            partyName: type === 'purchase'
                ? (inv.supplier?.supplierFullName || fb.partyName || '-')
                : (inv.project?.customer?.name || fb.partyName || '-'),
            secondaryRef: type === 'purchase'
                ? (inv.warehouse?.warehouseTitle || inv.warehouse?.title || fb.secondaryRef || '-')
                : (inv.project?.projectTitle || fb.secondaryRef || '-'),
            partyPhone: type === 'purchase'
                ? (inv.supplier?.phone || fb.partyPhone || '-')
                : (inv.project?.customer?.phone || fb.partyPhone || '-'),
            partyAddress: type === 'purchase'
                ? (inv.supplier?.address || '')
                : (inv.project?.customer?.address || ''),
            items: mappedItems,
            subtotal: Number(inv.subtotal ?? 0),
            discount: Number(inv.discount ?? 0),
            tax: Number(inv.tax ?? 0),
            total: totalVal,
            paidAmount,
            remainingAmount,
            notes: inv.notes || inv.description || '',
            company: branding.company
        };

        lastPreviewContext = { data: printData, template: branding.template };
        renderIntoHost(host, printData, branding.template);
    }

    function renderIntoHost(host, data, tpl) {
        const merged = mergeTemplate(tpl);
        host.innerHTML = `<style>${buildInvoiceCss(merged, false)}</style>${renderInvoiceBody(data, merged)}`;
    }

    function ensureDesignerModal() {
        if (document.getElementById('hscrmPrintDesignerModal')) return;
        const wrapper = document.createElement('div');
        wrapper.innerHTML = `
        <div class="modal fade" id="hscrmPrintDesignerModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header bg-primary text-white">
                        <h5 class="modal-title fw-bold"><i class="mdi mdi-palette-outline me-1"></i>طراح قالب چاپ فاکتورهای فروش و خرید</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row g-3">
                            <div class="col-lg-5">
                                <div class="card border h-100">
                                    <div class="card-header bg-light fw-bold small">تنظیمات ظاهر و اجزای فاکتور</div>
                                    <div class="card-body small" style="max-height: 68vh; overflow-y: auto;">
                                        <div class="row g-2 mb-2">
                                            <div class="col-6">
                                                <label class="form-label fw-bold">سبک قالب</label>
                                                <select id="pdTheme" class="form-select form-select-sm pd-input">
                                                    <option value="classic">رسمی کلاسیک</option>
                                                    <option value="corporate">شرکتی با هدر رنگی</option>
                                                    <option value="modern">مدرن مینیمال</option>
                                                </select>
                                            </div>
                                            <div class="col-6">
                                                <label class="form-label fw-bold">سایز کاغذ</label>
                                                <select id="pdPaperSize" class="form-select form-select-sm pd-input">
                                                    <option value="A4 portrait">A4 عمودی</option>
                                                    <option value="A4 landscape">A4 افقی</option>
                                                    <option value="A5 portrait">A5 عمودی</option>
                                                    <option value="A5 landscape">A5 افقی</option>
                                                </select>
                                            </div>
                                        </div>
                                        <div class="row g-2 mb-2">
                                            <div class="col-6">
                                                <label class="form-label fw-bold">رنگ اصلی</label>
                                                <div class="d-flex align-items-center gap-2">
                                                    <input type="color" id="pdAccentColor" class="form-control form-control-color pd-input" value="#1e40af" />
                                                    <button type="button" class="btn btn-sm btn-outline-primary pd-color-preset" data-color="#1e40af">آبی</button>
                                                    <button type="button" class="btn btn-sm btn-outline-success pd-color-preset" data-color="#15803d">سبز</button>
                                                    <button type="button" class="btn btn-sm btn-outline-dark pd-color-preset" data-color="#1e293b">سرمه‌ای</button>
                                                </div>
                                            </div>
                                            <div class="col-6">
                                                <label class="form-label fw-bold">اندازه فونت</label>
                                                <select id="pdFontSize" class="form-select form-select-sm pd-input">
                                                    <option value="11">کوچک (11px)</option>
                                                    <option value="12">استاندارد (12px)</option>
                                                    <option value="13">متوسط (13px)</option>
                                                    <option value="14">درشت (14px)</option>
                                                </select>
                                            </div>
                                        </div>
                                        <div class="row g-2 mb-2">
                                            <div class="col-6">
                                                <label class="form-label fw-bold">عنوان فاکتور فروش</label>
                                                <input type="text" id="pdSalesTitle" class="form-control form-control-sm pd-input" />
                                            </div>
                                            <div class="col-6">
                                                <label class="form-label fw-bold">عنوان فاکتور خرید</label>
                                                <input type="text" id="pdPurchaseTitle" class="form-control form-control-sm pd-input" />
                                            </div>
                                        </div>
                                        <div class="mb-2">
                                            <label class="form-label fw-bold">موقعیت لوگو</label>
                                            <select id="pdLogoPosition" class="form-select form-select-sm pd-input">
                                                <option value="right">سمت راست</option>
                                                <option value="center">وسط</option>
                                                <option value="left">سمت چپ</option>
                                            </select>
                                        </div>
                                        <hr class="my-2" />
                                        <div class="row g-2 mb-2">
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowLogo"><label class="form-check-label" for="pdShowLogo">نمایش لوگو</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowCompanyMeta"><label class="form-check-label" for="pdShowCompanyMeta">مشخصات شرکت</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowRowNumber"><label class="form-check-label" for="pdShowRowNumber">ستون ردیف</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowUnit"><label class="form-check-label" for="pdShowUnit">ستون واحد</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowDiscount"><label class="form-check-label" for="pdShowDiscount">ستون تخفیف</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowTax"><label class="form-check-label" for="pdShowTax">ستون مالیات</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowAmountInWords"><label class="form-check-label" for="pdShowAmountInWords">مبلغ به حروف</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowPaymentSummary"><label class="form-check-label" for="pdShowPaymentSummary">پرداختی و مانده</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowNotes"><label class="form-check-label" for="pdShowNotes">توضیحات فاکتور</label></div></div>
                                            <div class="col-6"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowFooterText"><label class="form-check-label" for="pdShowFooterText">پاورقی تنظیمات</label></div></div>
                                            <div class="col-12"><div class="form-check form-switch"><input class="form-check-input pd-input" type="checkbox" id="pdShowSignatures"><label class="form-check-label" for="pdShowSignatures">بخش امضا و مهر</label></div></div>
                                        </div>
                                        <div class="row g-2">
                                            <div class="col-4"><label class="form-label">امضای ۱</label><input type="text" id="pdSig1" class="form-control form-control-sm pd-input" /></div>
                                            <div class="col-4"><label class="form-label">امضای ۲</label><input type="text" id="pdSig2" class="form-control form-control-sm pd-input" /></div>
                                            <div class="col-4"><label class="form-label">امضای ۳</label><input type="text" id="pdSig3" class="form-control form-control-sm pd-input" /></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-7">
                                <div class="card border h-100">
                                    <div class="card-header bg-light d-flex justify-content-between align-items-center">
                                        <span class="fw-bold small">پیش‌نمایش زنده قالب فاکتور</span>
                                        <button type="button" class="btn btn-sm btn-outline-dark" id="pdTestPrintBtn">
                                            <i class="mdi mdi-printer me-1"></i>تست چاپ با Print.js
                                        </button>
                                    </div>
                                    <div class="card-body bg-white overflow-auto" style="max-height: 68vh;">
                                        <div id="hscrmPrintDesignerPreview"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer justify-content-between">
                        <button type="button" class="btn btn-outline-secondary" id="pdResetDefaultBtn">بازنشانی به پیش‌فرض</button>
                        <div class="d-flex gap-2">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">بستن</button>
                            <button type="button" class="btn btn-primary fw-bold" id="pdSaveTemplateBtn">
                                <i class="mdi mdi-content-save me-1"></i>ذخیره قالب فاکتور
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
        document.body.appendChild(wrapper.firstElementChild);
    }

    function readDesignerForm() {
        return {
            theme: document.getElementById('pdTheme').value,
            accentColor: document.getElementById('pdAccentColor').value,
            paperSize: document.getElementById('pdPaperSize').value,
            fontSize: parseInt(document.getElementById('pdFontSize').value, 10) || 12,
            salesTitle: document.getElementById('pdSalesTitle').value.trim() || DEFAULT_TEMPLATE.salesTitle,
            purchaseTitle: document.getElementById('pdPurchaseTitle').value.trim() || DEFAULT_TEMPLATE.purchaseTitle,
            logoPosition: document.getElementById('pdLogoPosition').value,
            showLogo: document.getElementById('pdShowLogo').checked,
            showCompanyMeta: document.getElementById('pdShowCompanyMeta').checked,
            showRowNumber: document.getElementById('pdShowRowNumber').checked,
            showUnit: document.getElementById('pdShowUnit').checked,
            showDiscount: document.getElementById('pdShowDiscount').checked,
            showTax: document.getElementById('pdShowTax').checked,
            showAmountInWords: document.getElementById('pdShowAmountInWords').checked,
            showPaymentSummary: document.getElementById('pdShowPaymentSummary').checked,
            showNotes: document.getElementById('pdShowNotes').checked,
            showFooterText: document.getElementById('pdShowFooterText').checked,
            showSignatures: document.getElementById('pdShowSignatures').checked,
            sig1Label: document.getElementById('pdSig1').value.trim() || DEFAULT_TEMPLATE.sig1Label,
            sig2Label: document.getElementById('pdSig2').value.trim() || DEFAULT_TEMPLATE.sig2Label,
            sig3Label: document.getElementById('pdSig3').value.trim() || DEFAULT_TEMPLATE.sig3Label
        };
    }

    function writeDesignerForm(tpl) {
        const t = mergeTemplate(tpl);
        document.getElementById('pdTheme').value = t.theme;
        document.getElementById('pdAccentColor').value = t.accentColor;
        document.getElementById('pdPaperSize').value = t.paperSize;
        document.getElementById('pdFontSize').value = String(t.fontSize);
        document.getElementById('pdSalesTitle').value = t.salesTitle;
        document.getElementById('pdPurchaseTitle').value = t.purchaseTitle;
        document.getElementById('pdLogoPosition').value = t.logoPosition;
        document.getElementById('pdShowLogo').checked = !!t.showLogo;
        document.getElementById('pdShowCompanyMeta').checked = !!t.showCompanyMeta;
        document.getElementById('pdShowRowNumber').checked = !!t.showRowNumber;
        document.getElementById('pdShowUnit').checked = !!t.showUnit;
        document.getElementById('pdShowDiscount').checked = !!t.showDiscount;
        document.getElementById('pdShowTax').checked = !!t.showTax;
        document.getElementById('pdShowAmountInWords').checked = !!t.showAmountInWords;
        document.getElementById('pdShowPaymentSummary').checked = !!t.showPaymentSummary;
        document.getElementById('pdShowNotes').checked = !!t.showNotes;
        document.getElementById('pdShowFooterText').checked = !!t.showFooterText;
        document.getElementById('pdShowSignatures').checked = !!t.showSignatures;
        document.getElementById('pdSig1').value = t.sig1Label;
        document.getElementById('pdSig2').value = t.sig2Label;
        document.getElementById('pdSig3').value = t.sig3Label;
    }

    async function openDesignerModal(opts) {
        ensureDesignerModal();
        const apiBase = String((opts && opts.apiBase) || '').replace(/\/+$/, '');
        const token = (opts && opts.token) || '';
        const branding = await fetchBrandingAndTemplate(apiBase, token);

        const sampleData = {
            invoiceKind: 'sales',
            number: 'INV-1405-0024',
            dateShamsi: '1405/08/25',
            statusText: 'فاکتور نهایی',
            partyName: 'شرکت ساختمانی آریا',
            secondaryRef: 'پروژه برج میلاد ۲',
            partyPhone: '021-88776655',
            partyAddress: 'تهران، خیابان ولیعصر، پلاک ۱۲۰',
            items: [
                { title: 'موتور گیرلس آسانسور ۱۰ نفره', unit: 'دستگاه', quantity: 1, unitPrice: 185000000, discount: 5000000, tax: 18000000, lineTotal: 198000000 },
                { title: 'تابلو فرمان ۳VF تمام‌اتوماتیک', unit: 'عدد', quantity: 1, unitPrice: 65000000, discount: 0, tax: 6500000, lineTotal: 71500000 }
            ],
            subtotal: 250000000,
            discount: 5000000,
            tax: 24500000,
            total: 269500000,
            paidAmount: 150000000,
            remainingAmount: 119500000,
            notes: 'تحویل در محل پروژه با گارانتی ۱۲ ماهه',
            company: branding.company
        };

        writeDesignerForm(branding.template);
        const previewHost = document.getElementById('hscrmPrintDesignerPreview');
        const refreshPreview = () => renderIntoHost(previewHost, sampleData, readDesignerForm());
        refreshPreview();

        document.querySelectorAll('#hscrmPrintDesignerModal .pd-input').forEach(el => {
            el.oninput = refreshPreview;
            el.onchange = refreshPreview;
        });
        document.querySelectorAll('#hscrmPrintDesignerModal .pd-color-preset').forEach(btn => {
            btn.onclick = function () {
                document.getElementById('pdAccentColor').value = btn.getAttribute('data-color');
                refreshPreview();
            };
        });

        document.getElementById('pdResetDefaultBtn').onclick = function () {
            writeDesignerForm(DEFAULT_TEMPLATE);
            refreshPreview();
        };

        document.getElementById('pdTestPrintBtn').onclick = function () {
            printInvoiceData(sampleData, readDesignerForm());
        };

        document.getElementById('pdSaveTemplateBtn').onclick = async function () {
            const tpl = readDesignerForm();
            saveLocalTemplate(tpl);
            if (opts && opts.canSaveServer && apiBase && token) {
                try {
                    const res = await fetch(`${apiBase}/Tenant/UpdatePrintTemplate`, {
                        method: 'PUT',
                        headers: {
                            'Authorization': 'Bearer ' + token,
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ invoicePrintTemplateJson: JSON.stringify(tpl) })
                    });
                    const json = await res.json();
                    if (!res.ok || !json || json.status === false) {
                        if (window.SwalHelper) SwalHelper.error((json && json.message) || 'خطا در ذخیره قالب روی سرور');
                        return;
                    }
                } catch {
                    if (window.SwalHelper) SwalHelper.error('خطا در ارتباط با سرور هنگام ذخیره قالب چاپ.');
                    return;
                }
            }
            if (window.SwalHelper) SwalHelper.success('قالب چاپ فاکتور با موفقیت ذخیره شد.');
            if (opts && typeof opts.onSaved === 'function') opts.onSaved(tpl);
            bootstrap.Modal.getInstance(document.getElementById('hscrmPrintDesignerModal'))?.hide();
        };

        bootstrap.Modal.getOrCreateInstance(document.getElementById('hscrmPrintDesignerModal')).show();
    }

    function renderInvoiceHtml(data, template) {
        const merged = mergeTemplate(template || getLocalTemplate());
        return `<style>${buildInvoiceCss(merged, false)}</style>${renderInvoiceBody(data, merged)}`;
    }

    window.HSCrmPrint = {
        DEFAULT_TEMPLATE,
        getTemplate: getLocalTemplate,
        mergeTemplate,
        renderInvoiceHtml,
        fetchBrandingAndTemplate,
        printInvoiceData,
        printElementHtml,
        openInvoicePrintModal,
        openDesignerModal
    };
})(window, document);
