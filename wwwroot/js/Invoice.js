// invoice.js - Client-side calculations for Invofy

document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('invoiceForm');
    const lineItemsContainer = document.getElementById('lineItemsContainer');
    const previewContainer = document.getElementById('previewContainer');

    // Main function to recalculate everything
    function calculateTotals() {
        let subtotal = 0;

        // Calculate each line total
        const rows = lineItemsContainer.querySelectorAll('tbody tr');
        rows.forEach(row => {
            const qtyInput = row.querySelector('input[name*="Quantity"]');
            const priceInput = row.querySelector('input[name*="UnitPrice"]');
            const lineTotalCell = row.querySelector('td.text-end.fw-bold');

            if (qtyInput && priceInput && lineTotalCell) {
                const qty = parseFloat(qtyInput.value) || 0;
                const price = parseFloat(priceInput.value) || 0;
                const lineTotal = qty * price;

                lineTotalCell.textContent = lineTotal.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD'   // Change if you support multiple currencies
                });

                subtotal += lineTotal;
            }
        });

        // Get tax and discount from form
        const taxRate = parseFloat(document.querySelector('input[name="TaxRate"]').value) || 0;
        const discount = parseFloat(document.querySelector('input[name="DiscountAmount"]').value) || 0;

        const taxAmount = subtotal * (taxRate / 100);
        const grandTotal = subtotal + taxAmount - discount;

        // Update preview (simple live update)
        updatePreviewTotals(subtotal, taxAmount, discount, grandTotal);
    }

    // Update totals displayed in the Live Preview
    function updatePreviewTotals(subtotal, taxAmount, discount, grandTotal) {
        if (!previewContainer) return;

        const subtotalEl = previewContainer.querySelector('#preview-subtotal');
        const taxEl = previewContainer.querySelector('#preview-tax');
        const discountEl = previewContainer.querySelector('#preview-discount');
        const grandTotalEl = previewContainer.querySelector('#preview-grandtotal');

        if (subtotalEl) subtotalEl.textContent = subtotal.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
        if (taxEl) taxEl.textContent = taxAmount.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
        if (discountEl) discountEl.textContent = discount.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
        if (grandTotalEl) grandTotalEl.textContent = grandTotal.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
    }

    // Event delegation - Listen to any change in the line items container
    if (lineItemsContainer) {
        lineItemsContainer.addEventListener('input', function (e) {
            // Only trigger on quantity and unit price inputs
            if (e.target.matches('input[name*="Quantity"], input[name*="UnitPrice"]')) {
                calculateTotals();
            }
        });

        // Also listen to tax and discount changes
        const taxInput = document.querySelector('input[name="TaxRate"]');
        const discountInput = document.querySelector('input[name="DiscountAmount"]');

        if (taxInput) taxInput.addEventListener('input', calculateTotals);
        if (discountInput) discountInput.addEventListener('input', calculateTotals);
    }

    // Trigger initial calculation when page loads
    setTimeout(calculateTotals, 300);

    // Optional: Trigger calculation after HTMX updates (after adding/removing rows)
    document.body.addEventListener('htmx:afterSwap', function (evt) {
        if (evt.detail.target.id === 'lineItemsContainer') {
            setTimeout(calculateTotals, 100);
        }
    });
});