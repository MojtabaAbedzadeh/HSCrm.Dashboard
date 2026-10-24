// =========================================================================
// HSCrm.Dashboard - Global SweetAlert2 Notification Engine & Ajax Handler
// =========================================================================

window.SwalHelper = {
    success: function (message, title, timer, callback) {
        title = title || "عملیات موفق";
        timer = timer || 1800;
        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (!swalInstance) return Promise.resolve();
        return swalInstance.fire({
            title: title,
            html: message,
            icon: "success",
            confirmButtonText: "باشه",
            confirmButtonColor: "#28a745",
            showConfirmButton: true,
            timer: timer,
            timerProgressBar: true,
            allowOutsideClick: false
        }).then(function (result) {
            if (typeof callback === "function") {
                callback(result);
            }
        });
    },

    error: function (message, title, callback) {
        title = title || "خطا در عملیات";
        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (!swalInstance) return Promise.resolve();
        return swalInstance.fire({
            title: title,
            html: message || "خطایی در انجام عملیات رخ داده است.",
            icon: "error",
            confirmButtonText: "متوجه شدم",
            confirmButtonColor: "#dc3545",
            showConfirmButton: true,
            allowOutsideClick: false
        }).then(function (result) {
            if (typeof callback === "function") {
                callback(result);
            }
        });
    },

    warning: function (message, title, callback) {
        title = title || "هشدار";
        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (!swalInstance) return Promise.resolve();
        return swalInstance.fire({
            title: title,
            html: message,
            icon: "warning",
            confirmButtonText: "باشه",
            confirmButtonColor: "#ffc107",
            showConfirmButton: true,
            allowOutsideClick: false
        }).then(function (result) {
            if (typeof callback === "function") {
                callback(result);
            }
        });
    },

    info: function (message, title, callback) {
        title = title || "اطلاعیه";
        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (!swalInstance) return Promise.resolve();
        return swalInstance.fire({
            title: title,
            html: message,
            icon: "info",
            confirmButtonText: "باشه",
            confirmButtonColor: "#17a2b8",
            showConfirmButton: true,
            allowOutsideClick: false
        }).then(function (result) {
            if (typeof callback === "function") {
                callback(result);
            }
        });
    },

    confirm: function (message, title, confirmCallback, cancelCallback) {
        title = title || "آیا مطمئن هستید؟";
        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (!swalInstance) return Promise.resolve();
        return swalInstance.fire({
            title: title,
            html: message,
            icon: "question",
            showCancelButton: true,
            confirmButtonColor: "#28a745",
            cancelButtonColor: "#d33",
            confirmButtonText: "بله، انجام شود",
            cancelButtonText: "انصراف",
            allowOutsideClick: false
        }).then(function (result) {
            if (result.isConfirmed) {
                if (typeof confirmCallback === "function") confirmCallback();
            } else if (result.dismiss === swalInstance.DismissReason.cancel) {
                if (typeof cancelCallback === "function") cancelCallback();
            }
        });
    },

    toast: function (message, icon) {
        icon = icon || "info";
        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (!swalInstance) return;
        const Toast = swalInstance.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true
        });
        return Toast.fire({
            icon: icon,
            title: message
        });
    },

    // پردازش پاسخ استاندارد ApiResponse بازگشتی از سرور
    handleResponse: function (response, successMsg, onSuccess) {
        if (!response) {
            this.error("پاسخی از سرور دریافت نشد.");
            return;
        }

        const isSuccess = response.status === true || response.statusCode === 200 || response.statusCode === "200";
        if (isSuccess) {
            const msg = response.message || successMsg || "عملیات با موفقیت انجام شد.";
            this.success(msg, "موفق", 1800, onSuccess);
            return;
        }

        // اگر عملیات ناموفق بود
        let errorMsg = response.message || "عملیات با خطا مواجه شد.";
        let errorTitle = "خطا";
        let iconType = "error";

        switch (parseInt(response.statusCode)) {
            case 204:
                errorTitle = "اطلاعات ناقص";
                iconType = "warning";
                if (!response.message) errorMsg = "هیچ آیتمی برای پردازش ارسال نشده است.";
                break;
            case 400:
                errorTitle = "درخواست نامعتبر";
                iconType = "warning";
                if (!response.message) errorMsg = "پارامترهای ارسالی نامعتبر است.";
                break;
            case 401:
                errorTitle = "عدم احراز هویت";
                iconType = "warning";
                errorMsg = "نشست کاربری شما منقضی شده است. لطفاً مجدداً وارد شوید.";
                break;
            case 403:
                errorTitle = "عدم دسترسی";
                iconType = "error";
                errorMsg = "شما مجوز انجام این عملیات را ندارید.";
                break;
            case 404:
                errorTitle = "یافت نشد";
                iconType = "warning";
                if (!response.message) errorMsg = "اطلاعات یا رکورد مورد نظر یافت نشد.";
                break;
            case 406:
                errorTitle = "کمبود موجودی";
                iconType = "warning";
                if (!response.message) errorMsg = "موجودی انبار برای کالای انتخابی کافی نمی‌باشد.";
                break;
            case 409:
                errorTitle = "اطلاعات تکراری";
                iconType = "warning";
                if (!response.message) errorMsg = "این اطلاعات قبلاً در سیستم ثبت شده است.";
                break;
            case 500:
                errorTitle = "خطای سرور";
                iconType = "error";
                if (!response.message) errorMsg = "خطای غیرمنتظره در سرور رخ داده است.";
                break;
            default:
                break;
        }

        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (swalInstance) {
            swalInstance.fire({
                title: errorTitle,
                html: errorMsg,
                icon: iconType,
                confirmButtonText: "باشه",
                confirmButtonColor: "#3085d6",
                allowOutsideClick: false
            });
        }
    }
};

// =========================================================================
// گلوبال اینترسپتور: به دام انداختن تمامی خطاهای AJAX (HTTP 4xx و 5xx)
// =========================================================================
if (typeof jQuery !== 'undefined') {
    $(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
        // جلوگیری از نمایش خطا برای درخواست‌های کنسل‌شده یا abort یا خودکار
        if (jqXHR.statusText === 'abort' || jqXHR.status === 0) {
            return;
        }

        // اگر درخواست دارای فلگ suppressGlobalAlert باشد
        if (ajaxSettings && ajaxSettings.suppressGlobalAlert) {
            return;
        }

        let errorTitle = "خطا";
        let errorMessage = "خطایی در انجام عملیات یا ارتباط با سرور رخ داده است.";
        let iconType = "error";

        // استخراج پیام سرور از JSON
        if (jqXHR.responseJSON) {
            const res = jqXHR.responseJSON;
            if (typeof res === 'string') {
                errorMessage = res;
            } else if (res.message) {
                errorMessage = res.message;
            } else if (res.title) {
                errorMessage = res.title;
            } else if (res.errors) {
                // خطاهای اعتبارسنجی فیلدهای فرم (Validation Errors)
                let errorList = [];
                for (let key in res.errors) {
                    if (Array.isArray(res.errors[key])) {
                        errorList.push(res.errors[key].join(' '));
                    } else {
                        errorList.push(res.errors[key]);
                    }
                }
                if (errorList.length > 0) {
                    errorMessage = errorList.join('<br/>');
                }
            }
        } else if (jqXHR.responseText) {
            try {
                const parsed = JSON.parse(jqXHR.responseText);
                if (parsed.message) errorMessage = parsed.message;
            } catch (e) {
                if (jqXHR.responseText.length < 200 && !jqXHR.responseText.includes('<html')) {
                    errorMessage = jqXHR.responseText;
                }
            }
        }

        switch (jqXHR.status) {
            case 400:
                errorTitle = "درخواست نامعتبر";
                iconType = "warning";
                break;
            case 401:
                errorTitle = "عدم احراز هویت";
                errorMessage = "نشست کاربری شما منقضی شده است. لطفاً مجدداً وارد شوید.";
                iconType = "warning";
                break;
            case 403:
                errorTitle = "عدم دسترسی";
                errorMessage = "شما مجوز انجام این عملیات را ندارید.";
                iconType = "error";
                break;
            case 404:
                errorTitle = "یافت نشد";
                iconType = "warning";
                break;
            case 406:
                errorTitle = "کمبود موجودی";
                iconType = "warning";
                break;
            case 409:
                errorTitle = "اطلاعات تکراری";
                iconType = "warning";
                break;
            case 500:
                errorTitle = "خطای سرور";
                iconType = "error";
                break;
            default:
                errorTitle = "خطای ارتباط با سرور";
                break;
        }

        const swalInstance = (typeof Swal !== 'undefined') ? Swal : ((typeof swal !== 'undefined' && swal.fire) ? swal : null);
        if (swalInstance) {
            swalInstance.fire({
                title: errorTitle,
                html: errorMessage,
                icon: iconType,
                confirmButtonText: "متوجه شدم",
                confirmButtonColor: "#3085d6",
                allowOutsideClick: false
            });
        }
    });
}
