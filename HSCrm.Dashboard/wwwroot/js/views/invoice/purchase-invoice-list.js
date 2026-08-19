"use strict";

const ApiAddress = AppContext.apiAddress;

function getAuthToken() {
    return (window.AppContext && window.AppContext.token) ? window.AppContext.token : localStorage.getItem("token");
}

function handleSuccessReload() {
    if (typeof window.reloadInvoiceTable === "function") {
        window.reloadInvoiceTable();
    } else {
        setTimeout(function () {
            location.reload();
        }, 1000);
    }
}

function getAjaxHeaders() {
    var token = getAuthToken();
    return token ? { "Authorization": "Bearer " + token } : {};
}

function ajaxPostJson(url, data, onSuccess, onError) {
    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: getAjaxHeaders(),
        data: data !== undefined ? JSON.stringify(data) : null,
        success: function (res) {
            if (typeof onSuccess === "function") onSuccess(res);
        },
        error: function (xhr) {
            if (typeof onError === "function") onError(xhr);
        }
    });
}

function showErrorMessage(xhr, fallbackMessage) {
    var msg = fallbackMessage || "خطا در ارتباط با سرور";
    try {
        if (xhr && xhr.responseJSON && (xhr.responseJSON.message || xhr.responseJSON.Message)) {
            msg = xhr.responseJSON.message || xhr.responseJSON.Message;
        }
    } catch (e) { }

    Swal.fire({
        icon: "error",
        title: "خطا",
        text: msg,
        confirmButtonText: "باشه"
    });
}

function isSuccessResponse(res) {
    return res && (res.status === true || res.Status === true);
}

function showLoadingMessage(title, text) {
    Swal.fire({
        title: title || "در حال ارسال...",
        text: text || "لطفاً صبر کنید",
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        didOpen: function () {
            Swal.showLoading();
        }
    });
}

// 1) تأیید نهایی فاکتور خرید
window.confirmPurchaseInvoice = function (id) {
    Swal.fire({
        title: "تأیید نهایی فاکتور",
        text: "آیا از تأیید نهایی این فاکتور اطمینان دارید؟",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "بله، تأیید شود",
        cancelButtonText: "انصراف",
        reverseButtons: true,
        focusCancel: true
    }).then(function (result) {
        if (!result.isConfirmed) return;

        var apiUrl = ApiAddress + "PurchaseInvoice/ConfirmInvoice/" + id;

        showLoadingMessage("در حال ارسال...", "لطفاً صبر کنید");

        ajaxPostJson(
            apiUrl,
            null,
            function (res) {
                if (isSuccessResponse(res)) {
                    Swal.fire({
                        icon: "success",
                        title: "موفق",
                        text: res.message || res.Message || "فاکتور تأیید شد.",
                        confirmButtonText: "باشه"
                    }).then(function () {
                        handleSuccessReload();
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "خطا",
                        text: (res && (res.message || res.Message)) || "عملیات با خطا مواجه شد.",
                        confirmButtonText: "باشه"
                    });
                }
            },
            function (xhr) {
                showErrorMessage(xhr, "خطا در برقراری ارتباط با سرور");
            }
        );
    });
};

// 2) ابطال فاکتور خرید
window.cancelPurchaseInvoice = function (id) {
    Swal.fire({
        title: "ابطال فاکتور خرید",
        input: "text",
        inputLabel: "لطفاً دلیل ابطال فاکتور را وارد نمایید",
        inputPlaceholder: "علت ابطال...",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "ابطال فاکتور",
        cancelButtonText: "انصراف",
        reverseButtons: true,
        focusCancel: true,
        inputAttributes: {
            autocapitalize: "off",
            autocorrect: "off"
        },
        preConfirm: function (value) {
            var reason = (value || "").trim();
            if (!reason) {
                Swal.showValidationMessage("وارد کردن دلیل ابطال الزامی است!");
                return false;
            }
            return reason;
        }
    }).then(function (result) {
        if (!result.isConfirmed) return;

        var reason = result.value;
        var apiUrl = ApiAddress + "PurchaseInvoice/CancelInvoice/" + id;

        showLoadingMessage("در حال ارسال...", "لطفاً صبر کنید");

        ajaxPostJson(
            apiUrl,
            reason,
            function (res) {
                if (isSuccessResponse(res)) {
                    Swal.fire({
                        icon: "success",
                        title: "ابطال شد",
                        text: res.message || res.Message || "فاکتور خرید با موفقیت باطل شد.",
                        confirmButtonText: "باشه"
                    }).then(function () {
                        handleSuccessReload();
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "خطا",
                        text: (res && (res.message || res.Message)) || "عملیات ابطال ناموفق بود.",
                        confirmButtonText: "باشه"
                    });
                }
            },
            function (xhr) {
                showErrorMessage(xhr, "خطا در ارتباط با سرور");
            }
        );
    });
};

// 3) رد فاکتور خرید
window.rejectPurchaseInvoice = function (id) {
    Swal.fire({
        title: "رد فاکتور خرید",
        input: "text",
        inputLabel: "لطفاً دلیل رد فاکتور را وارد نمایید",
        inputPlaceholder: "علت رد...",
        showCancelButton: true,
        confirmButtonColor: "#ffc107",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "رد فاکتور",
        cancelButtonText: "انصراف",
        reverseButtons: true,
        focusCancel: true,
        inputAttributes: {
            autocapitalize: "off",
            autocorrect: "off"
        },
        preConfirm: function (value) {
            var reason = (value || "").trim();
            if (!reason) {
                Swal.showValidationMessage("وارد کردن دلیل رد الزامی است!");
                return false;
            }
            return reason;
        }
    }).then(function (result) {
        if (!result.isConfirmed) return;

        var reason = result.value;
        var apiUrl = ApiAddress + "PurchaseInvoice/RejectInvoice/" + id;
        console.log("New APIUrl:" + apiUrl);

        showLoadingMessage("در حال ارسال...", "لطفاً صبر کنید");

        ajaxPostJson(
            apiUrl,
            reason,
            function (res) {
                if (isSuccessResponse(res)) {
                    Swal.fire({
                        icon: "success",
                        title: "رد شد",
                        text: res.message || res.Message || "فاکتور خرید رد شد.",
                        confirmButtonText: "باشه"
                    }).then(function () {
                        handleSuccessReload();
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "خطا",
                        text: (res && (res.message || res.Message)) || "عملیات رد فاکتور ناموفق بود.",
                        confirmButtonText: "باشه"
                    });
                }
            },
            function (xhr) {
                showErrorMessage(xhr, "خطا در ارتباط با سرور");
            }
        );
    });
};
