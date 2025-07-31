
//For Printing receipt in the modal
function printModal() {
    const modalBody = document.querySelector('#printReceipt .modal-body').innerHTML;
    const printWindow = window.open('', '', 'width=800,height=600');
    printWindow.document.write(`
        <html>
            <head>
                <title>Print Purchase Order</title>
                <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                <style>
                    @media print {
                        @page {
                            size: portrait;
                        }
                        body {
                            -webkit-print-color-adjust: exact;
                            print-color-adjust: exact;
                        }
                    }
                </style>
            </head>
            <body onload="window.print(); window.close();">
                ${modalBody}
            </body>
        </html>
    `);
    printWindow.document.close();
}

function printModalquarter() {
    const modalBody = document.querySelector('.modal-body').innerHTML;
    const printWindow = window.open('', '', 'width=800,height=600');
    printWindow.document.write(`
        <html>
            <head>
                <title>Print Purchase Order</title>
                <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                <style>
                    @media print {
                        @page {
                            size: A4;
                            margin: 0;
                        }
                        body {
                            margin: 0;
                            height: 100vh;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            -webkit-print-color-adjust: exact;
                            print-color-adjust: exact;
                        }
                        .scaled-container {
                            transform: scale(0.5); /* Scale down to 50% */
                            transform-origin: top left;
                            width: 210mm; /* Full A4 width */
                            height: 297mm; /* Full A4 height */
                            padding: 10mm;
                        }
                    }
                </style>
            </head>
            <body onload="window.print(); window.close();">
                <div class="scaled-container">
                    ${modalBody}
                </div>
            </body>
        </html>
    `);
    printWindow.document.close();
}

function emailModal() {
    const modalBodyHtml = document.querySelector('.modal-body').innerHTML;
    const toEmail = prompt("Enter recipient email:");

    if (!toEmail) return;

    fetch('?handler=EmailModal', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            email: toEmail,
            html: modalBodyHtml
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Email sent successfully!");
            } else {
                alert("Error sending email: " + data.error);
            }
        });
}

// ========== TODAY'S SALES POPUP ==========

// Show popup with loader only (used before content fetch)
function showSalesPopup() {
    const popup = document.getElementById("todaySalesPopup");
    const content = document.getElementById("todaySalesContent");

    if (!popup || !content) {
        console.error("Popup or content container not found.");
        return;
    }

    // ✅ Fade-in via CSS class
    popup.classList.add("show");

    content.innerHTML = `
        <div class="text-center my-3" id="salesLoader">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    `;

    fetch("/TodaySales?handler=Partial", {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    })
        .then(res => res.text())
        .then(html => {
            content.innerHTML = html;
            bindOfficeChangeEvent();
            submitTodaySalesForm();
        })
        .catch(err => {
            console.error("Error loading initial popup content:", err);
            content.innerHTML = "<div class='alert alert-danger'>Failed to load popup content.</div>";
        });
}

function closeSalesPopup() {
    const popup = document.getElementById("todaySalesPopup");
    if (popup) popup.classList.remove("show"); // ✅ fade-out
}



// Hide the popup
function closeSalesPopup() {
    const popup = document.getElementById("todaySalesPopup");
    if (popup) popup.classList.remove("show");
}

// Submit the office filter form via AJAX
function submitTodaySalesForm() {
    const form = document.querySelector("#todaySalesContent #salesFilterForm");
    const select = document.querySelector("#todaySalesContent #OfficeId");
    const content = document.getElementById("todaySalesContent");

    if (!form || !select || !content) {
        console.error("Form, select, or content not found in submitTodaySalesForm().");
        return;
    }

    const selectedOption = select.options[select.selectedIndex];
    const currency = selectedOption.getAttribute("data-currency") || "₱";

    content.innerHTML = `
        <div class="text-center my-2" id="salesLoader">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    `;
    content.style.opacity = 0.5;

    const formData = new FormData(form);
    const queryString = new URLSearchParams(formData).toString();

    fetch(`/TodaySales?handler=Partial&${queryString}`, {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    })
        .then(response => response.text())
        .then(html => {
            content.innerHTML = html;
            content.style.opacity = 1;

            const currencySymbolEl = document.getElementById("currencySymbol");
            if (currencySymbolEl) {
                currencySymbolEl.innerText = currency;
            }

            bindOfficeChangeEvent(); // Rebind again after refresh
        })
        .catch(error => {
            console.error("Error loading sales data:", error);
            content.innerHTML = "<div class='alert alert-danger'>Failed to load data.</div>";
            content.style.opacity = 1;
        });
}


// Bind onchange event to Office dropdown
function bindOfficeChangeEvent() {
    const select = document.querySelector("#todaySalesContent #OfficeId");
    if (select) {
        select.removeEventListener("change", submitTodaySalesForm);
        select.addEventListener("change", submitTodaySalesForm);
    } else {
        console.warn("OfficeId not found during bind.");
    }
}


// Enable dragging of popup via header
function makeDraggable(popupId, handleId) {
    const popup = document.getElementById(popupId);
    const handle = document.getElementById(handleId);

    if (!popup || !handle) return;

    let offsetX = 0, offsetY = 0, isDragging = false;

    handle.addEventListener('mousedown', function (e) {
        isDragging = true;
        const rect = popup.getBoundingClientRect();
        offsetX = e.clientX - rect.left;
        offsetY = e.clientY - rect.top;

        document.addEventListener('mousemove', dragMove);
        document.addEventListener('mouseup', dragEnd);
    });

    function dragMove(e) {
        if (!isDragging) return;
        popup.style.position = 'fixed';
        popup.style.left = `${e.clientX - offsetX}px`;
        popup.style.top = `${e.clientY - offsetY}px`;
    }

    function dragEnd() {
        isDragging = false;
        document.removeEventListener('mousemove', dragMove);
        document.removeEventListener('mouseup', dragEnd);
    }
}

// ========== INIT ==========

document.addEventListener("DOMContentLoaded", function () {
    makeDraggable("todaySalesPopup", "salesPopupHeader");
});

// fades out automatically
setTimeout(() => {
    closeSalesPopup();
}, 5000);

document.addEventListener('DOMContentLoaded', function () {
    const modalEl = document.getElementById('changebgModal');
    modalEl.addEventListener('shown.bs.modal', function () {
        initializeBackgroundSelector();
    });
});


function initializeBackgroundSelector() {
    const thumbs = document.querySelectorAll('.bg-thumb');
    const hiddenInput = document.getElementById('selectedBackground');
    const saveButton = document.getElementById('saveBgBtn');

    if (!thumbs.length || !hiddenInput || !saveButton) return;

    thumbs.forEach(thumb => {
        thumb.addEventListener('click', function () {
            const imgUrl = this.src;
            document.body.style.backgroundImage = `url('${imgUrl}')`;
            document.body.style.backgroundSize = 'cover';
            document.body.style.backgroundPosition = 'center';

            hiddenInput.value = imgUrl;
        });
    });

    saveButton.addEventListener('click', function () {
        const selected = hiddenInput.value;
        if (!selected) {
            Swal.fire({
                icon: 'warning',
                title: 'Select First',
                text: 'Please select a background first.',
            });
            return;
        }
        showLoader();
        fetch('/Utilities/ChangeBackground/_ChangeBackgroundPartial?handler=SaveBackground', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ selectedBackground: selected })
        })
            .then(res => {
                if (res.ok) {
                    hideLoader();
                    Swal.fire({
                        icon: 'success',
                        title: 'Applied!',
                        text: 'Background applied successfully.',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    const modalEl = document.getElementById("changebgModal");
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    modal.hide();
                } else {
                    hideLoader();
                    Swal.fire({
                        icon: 'error',
                        title: 'Failed',
                        text: 'Failed to save background.',
                    });
                }
            });
    });
}

//Right click
document.addEventListener('DOMContentLoaded', function () {
    const contextMenu = document.getElementById('customContextMenu');

    document.body.addEventListener('contextmenu', function (e) {
        //return; //temporary disable for debugging
        e.preventDefault();

        contextMenu.style.left = `${e.pageX}px`;
        contextMenu.style.top = `${e.pageY}px`;
        contextMenu.classList.remove('d-none');
    });

    document.body.addEventListener('click', function () {
        contextMenu.classList.add('d-none');
    });
});

//Change theme
const themeMap = {
    default: "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css",
    pulse: "https://cdn.jsdelivr.net/npm/bootswatch@5.3.2/dist/pulse/bootstrap.min.css",
    flatly: "https://cdn.jsdelivr.net/npm/bootswatch@5.3.2/dist/flatly/bootstrap.min.css",
    lumen: "https://cdn.jsdelivr.net/npm/bootswatch@5.3.2/dist/lumen/bootstrap.min.css"
};

function setTheme(name) {
    showLoader();
    const link = document.getElementById('bootstrap-theme');
    if (!link || !themeMap[name]) return;

    link.href = themeMap[name];
    localStorage.setItem("theme", name);
    updateThemeCheck(name);
    
}

function updateThemeCheck(selected) {
    // Hide all checks first
    Object.keys(themeMap).forEach(t => {
        const checkIcon = document.getElementById(`check-${t}`);
        if (checkIcon) checkIcon.classList.add('d-none');
    });

    // Show selected
    const selectedIcon = document.getElementById(`check-${selected}`);
    if (selectedIcon) selectedIcon.classList.remove('d-none');

    hideLoader();
}

// Load theme on startup
document.addEventListener("DOMContentLoaded", function () {
    const savedTheme = localStorage.getItem("theme") || "default";
    setTheme(savedTheme);
});


//Fixed the remaining backdrop of 2 modals
$('#addAccessModal').on('hidden.bs.modal', function () {
    // Check if the parent modal is still visible
    if ($('#officeAccessModal').hasClass('show')) {
        // Restore the modal-open class to prevent body scrolling
        $('body').addClass('modal-open');

        // Manually add a backdrop if it was removed
        if ($('.modal-backdrop').length === 0) {
            $('<div class="modal-backdrop fade show"></div>').appendTo(document.body);
        }
    }
});

function startClock(clockElementId) {
    function updateClock() {
        const now = new Date();
        const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

        const localTime = now.toLocaleString(undefined, {
            timeZone: timeZone,
            year: 'numeric',
            month: 'long',    // e.g., July
            day: '2-digit',
            weekday: 'long',  // e.g., Tuesday
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: true      // change to false for 24-hour format
        });

        const clockElement = document.getElementById(clockElementId);
        if (clockElement) {
            /* clockElement.textContent = ` ${localTime} (${timeZone})`;*/
            clockElement.textContent = ` ${localTime}`;
        }
    }

    updateClock();
    setInterval(updateClock, 1000);
}

document.addEventListener('keydown', function (event) {
    // F2 key code is 113
    if (event.key === "F6" || event.keyCode === 115) {
        event.preventDefault(); // Prevent browser default behavior (optional)

        const modal = new bootstrap.Modal(document.getElementById('changebgModal'));
        modal.show();
    }
});