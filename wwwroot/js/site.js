//For Printing receipt in the modal
function printModal() {
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

