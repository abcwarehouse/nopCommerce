// Globals
const cartSlideoutBackButton = document.querySelector('.cart-slideout__back');

const CartSlideoutOverlay = document.getElementsByClassName("cart-slideout-overlay")[0];
const CartSlideout = document.getElementsByClassName("cart-slideout")[0];

const CartSlideoutProductInfo = document.getElementsByClassName("cart-slideout__product-info")[0];

const deliveryInput = document.getElementsByClassName("cart-slideout__delivery-input")[0];
const zipCodeInput = document.getElementsByClassName('cart-slideout__delivery-zip-code-input')[0];
const checkButton = document.querySelector(".cart-slideout__check-delivery-options__btn");

const deliveryOptions = document.querySelector('.cart-slideout__delivery-options');
const deliveryNotAvailable = document.querySelector('.cart-slideout__delivery-not-available');

const deliveryOptionsInformation = document.querySelector('.delivery-options-information');
const warrantyInformation = document.querySelector('.warranty-information');
const addToCartButton = document.querySelector('#cart-slideout__add-to-cart');

const title = document.querySelector('.cart-slideout__title');
const goToCartButton = document.querySelector('#cart-slideout__go-to-cart');
const continueShoppingButton = document.querySelector('#cart-slideout__continue-shopping');

var cartSlideoutShoppingCartItemId = 0;
var productId = 0;
var editMode = false;
var isMattress = false;
var selectedShop = "";
var isPickupExcluded = false;

// Event listeners for updating the check delivery options button:
zipCodeInput.addEventListener('keyup', updateCheckDeliveryAvailabilityButton);
zipCodeInput.addEventListener('keypress', function(event) {
    if (event.key === "Enter") {
      event.preventDefault();
      checkDeliveryShippingAvailabilityAsync();
    }
});
CartSlideoutOverlay.addEventListener('click', hideCartSlideout);

function hideDeliveryOptionsInformation() {
    deliveryOptionsInformation.style.display = "none";
}

function hideWarrantyInformation() {
    warrantyInformation.style.display = "none";
}

async function checkDeliveryShippingAvailabilityAsync() {
    zipCodeInput.disabled = true;
    checkButton.disabled = true;

    const zip = zipCodeInput.value;
    document.cookie = `customerZipCode=${zip}`;
    const response = await fetch(`/AddToCart/GetDeliveryOptions?zip=${zip}&productId=${productId}`);
    if (response.status != 200) {
        alert('Error occurred when checking delivery options.');
        updateCheckDeliveryAvailabilityButton();
        return;
    }

    const responseJson = await response.json();
    openDeliveryOptions(responseJson);
    if (responseJson.pickupInStoreHtml) {
        $('.cart-slideout__pickup-in-store').html(responseJson.pickupInStoreHtml);
    }
    setInformationalIconListeners();
    updateCheckDeliveryAvailabilityButton();
    updateAttributes();
}

function openDeliveryOptions(response) {
    deliveryInput.style.display = "none";
    deliveryNotAvailable.style.display = "none";
    deliveryOptions.style.display = "none";

    if (response.isDeliveryAvailable || response.isFedExAvailable) {
        deliveryOptions.style.display = "block";
    } else {
        addToCartButton.disabled = true;
        deliveryNotAvailable.style.display = "block";
    }

    isPickupExcluded = !response.isDeliveryAvailable && response.isFedExAvailable;

    cartSlideoutBackButton.style.display = "block";
}

function updateCheckDeliveryAvailabilityButton() {
    if (zipCodeInput === undefined) { return; }
  
    const isNumber = /^\d+$/.test(zipCodeInput.value);
  
    zipCodeInput.disabled = false;
    checkButton.disabled = !isNumber || zipCodeInput.value.length !== 5;
    checkButton.innerText = "Check Delivery/Shipping Availability";
}

function showCartSlideout(response) {
    updateCartSlideoutHtml(response);

    CartSlideoutOverlay.style.display = "block";
    CartSlideout.style.display = "flex";
    addToCartButton.disabled = true;

    document.body.classList.add("scrollYRemove");
}

function clickGoToCart() {
    if (editMode) {
        location.reload();
    } else {
        window.location.href = '/cart'
    }
}

function hideCartSlideout() {
    if (editMode || isMattress) {
        location.reload();
    } else {
        CartSlideoutOverlay.style.display = "none";
        CartSlideout.style.display = "none";
        deliveryOptions.style.display = "none";
        deliveryNotAvailable.style.display = "none";
        cartSlideoutBackButton.style.display = "none";

        hideDeliveryOptionsInformation();
        hideWarrantyInformation();

        addToCartButton.disabled = true;

        addToCartButton.style.display = "block";
        title.style.display = "none";
        goToCartButton.style.display = "none";
        continueShoppingButton.style.display = "none";

        document.body.classList.remove("scrollYRemove");
    }
}

function back() {
    addToCartButton.disabled = true;

    deliveryNotAvailable.style.display = "none";
    deliveryOptions.style.display = "none";
    cartSlideoutBackButton.style.display = "none";

    deliveryInput.style.display = "block";
}

function updateCartSlideoutHtml(response) {
    if (response.slideoutInfo.ProductInfoHtml) {
        $('.cart-slideout__product-info').html(response.slideoutInfo.ProductInfoHtml);
    }

    // Should only check zip code if product has delivery options
    if (response.slideoutInfo.DeliveryOptionsHtml !== "") {
        deliveryInput.style.display = "block";
        zipCodeInput.value = getCookie('customerZipCode');
        updateCheckDeliveryAvailabilityButton();
        $('.cart-slideout__delivery-options').html(response.slideoutInfo.DeliveryOptionsHtml);
        setAttributeListeners(response.slideoutInfo.ShoppingCartItemId);
        productId = response.slideoutInfo.ProductId;
        cartSlideoutShoppingCartItemId = response.slideoutInfo.ShoppingCartItemId;
    } else {
        addToCartButton.style.display = "none";
        title.style.display = "block";
        goToCartButton.style.display = "block";
    }
}

async function selectStoreAsync(shopId, message)
{
    resetSelectStoreButtons();

    var selectedElement = document.querySelector(`#select_store_${shopId}`);
    selectedElement.classList.add("selected");

    selectedShop = `${shopId};${message}`

    updateAttributes();
}

function setAttributeListeners() {
    setInformationalIconListeners();

    var deliveryOptions = document.querySelectorAll('.cart-slideout__delivery-options [name^=product_attribute_]');
    for (option in deliveryOptions) {
        deliveryOptions[option].onclick = function() { updateAttributes(); };
    }
}

function preSelectNoWarrantyIfNoneSelected() {
    var warrantyOptions = document.getElementsByClassName('is-warranty');
    if (warrantyOptions.length == 0) { return; }

    var selectedWarrantyOption = Array.from(warrantyOptions).filter(function(option) {
        return option.checked;
    });
    // no warranty selected, select "No additional warranty"
    if (selectedWarrantyOption.length === 0) {
        var noWarrantyOption = document.getElementById('warranty-none');
        noWarrantyOption.checked = true;
    }
}

function updateAttributes() {
    setHaulawayCheckboxes();
    if (editMode) {
        preSelectNoWarrantyIfNoneSelected();
    }

    fetch(`/slideout_attributechange?productId=${productId}`, {
        method: 'POST',
        headers: {
            'content-type': 'application/x-www-form-urlencoded; charset=UTF-8'
        },
        body: $('#delivery-options').serialize()
    })
    .then(response => response.json())
    .then(responseJson => {
        isMattress = responseJson.IsMattress;

        // update possible conditional options
        responseJson.DisabledAttributeMappingIds.forEach(id => {
            var options = document.querySelectorAll(`[id$='_${id}']`);
            options.forEach(option => {
                option.style.display = "none";
                // Uncheck the hidden option
                var checkbox = option.querySelector(`input[id^='product_attribute_${id}_']`);
                if (checkbox !== undefined && checkbox !== null) {
                    checkbox.checked = false;
                }
            })
        });

        responseJson.EnabledAttributeMappingIds.forEach(id => {
            var options = document.querySelectorAll(`[id$='_${id}']`);
            options.forEach(option => {
                option.style.display = "block";
            })
        });

        const pickupInStoreOptions = document.querySelector('.cart-slideout__pickup-in-store');
        if (pickupInStoreOptions !== null) {
            pickupInStoreOptions.style.display = responseJson.IsPickup ?
                "block" :
                "none";
        }

        if (!responseJson.IsPickup) {
            resetSelectStoreButtons();
            selectedShop = "";
        }

        addToCartButton.disabled =
            !responseJson.IsAddEditCartAllowed ||
            (responseJson.IsPickup && selectedShop === "");

        if (isPickupExcluded) {
            var labels = document.querySelectorAll(`label[for^="product_attribute_"`);
            labels.forEach(label => {
                if (label.innerText.toLowerCase().includes('pickup in-store')) {
                    label.parentElement.style.display = "none";
                }
            });
        }
    })
    .catch(err => {
        console.log(err)
    })
}

function setHaulawayCheckboxes() {
    var checkboxes = document.getElementsByClassName('is-haulaway');
    var nonCheckedCheckboxes = Array.from(checkboxes).filter(function(checkbox) {
        return !checkbox.checked;
    });

    Array.from(checkboxes).forEach(function(cb) {
        cb.disabled = false;
    });

    if (checkboxes.length === nonCheckedCheckboxes.length) { return; }

    nonCheckedCheckboxes.forEach(function(cb) {
        cb.disabled = true;
    });
}

function setInformationalIconListeners() {
    var deliveryOptionInformationIcons = document.querySelectorAll('i.delivery-options-info');
    var warrantyInformationIcon = document.querySelector('i.warranty-info');
    for (icon in deliveryOptionInformationIcons) {
        deliveryOptionInformationIcons[icon].onclick = function() {
            deliveryOptionsInformation.style.display = "block";
        }
    }

    if (warrantyInformationIcon !== null) {
        warrantyInformationIcon.onclick = function () {
            warrantyInformation.style.display = "block";
        };
    }
}

async function editCartItemAsync(shoppingCartItemId) {
    editMode = true;
    addToCartButton.value = "Update Item";

    AjaxCart.setLoadWaiting(true);
    const response = await fetch(`/AddToCart/GetEditCartItemInfo?shoppingCartItemId=${shoppingCartItemId}`);
    if (response.status != 200) {
        alert('Error occurred when editing cart item.');
        AjaxCart.setLoadWaiting(false);
        return;
    }
    AjaxCart.setLoadWaiting(false);

    const responseJson = await response.json();
    showCartSlideout(responseJson);

    deliveryInput.style.display = "none";
    deliveryOptions.style.display = "block";
    updateAttributes();
}

async function addCartItemAsync(productId) {
    AjaxCart.setLoadWaiting(true);
    const response = await fetch(`/AddToCart/GetAddCartItemInfo?productId=${productId}`, {
        method: 'POST',
        headers: {
            'content-type': 'application/x-www-form-urlencoded; charset=UTF-8'
        },
        body: $('#product-details-form').serialize()
    });
    if (response.status != 200) {
        alert('Error occurred when getting delivery information.');
        AjaxCart.setLoadWaiting(false);
        return;
    }
    AjaxCart.setLoadWaiting(false);

    const responseJson = await response.json();
    showCartSlideout(responseJson);
}

function getCookie(cookieName) {
    let cookie = {};
    document.cookie.split(';').forEach(function(el) {
      let [key,value] = el.split('=');
      cookie[key.trim()] = value;
    })
    return cookie[cookieName] ?? '';
}

function AddToCart()
{
    console.log('=== AddToCart function started ===');
    
    cartSlideoutBackButton.style.display = "none";
    deliveryOptions.style.display = "none";
    addToCartButton.disabled = true;

    var payload = $('#delivery-options, #product-details-form').serialize();
    if (selectedShop != "")
    {
        payload += `&selectedShopId=${selectedShop}`;
    }
    if (cartSlideoutShoppingCartItemId != 0)
    {
        payload += `&addtocart_${productId}.UpdatedShoppingCartItemId=${cartSlideoutShoppingCartItemId}`;
    }

    console.log('AJAX request details:');
    console.log('- URL:', `/CartSlideout/AddToCartWithListrak/${productId}`);
    console.log('- Payload:', payload);

    $.ajax({
        cache: false,
        url: `/CartSlideout/AddToCartWithListrak/${productId}`, // Updated to use your custom endpoint
        data: payload,
        type: "POST",
        success: function(res) {
            console.log('=== AJAX SUCCESS ===');
            console.log('Raw response:', res);
            
            var response;
            try {
                response = (typeof res === "string") ? JSON.parse(res) : res;
                console.log('Parsed response:', response);
            } catch (parseError) {
                console.error('Error parsing response:', parseError);
                response = res;
            }

            if (!response.success) {
                alert(response.message || 'Error adding item to cart');
                cartSlideoutBackButton.style.display = "block";
                deliveryOptions.style.display = "block";
                addToCartButton.disabled = false;
                return;
            }

            addToCartButton.style.display = "none";
            title.style.display = "block";
            goToCartButton.style.display = "block";
            
            if (editMode) {
                title.innerHTML = "<i class='fas fa-check-circle'></i> Item Updated";
            } else {
                continueShoppingButton.style.display = "block";
            }

            // ---- Listrak Order API tracking ----
            console.log('=== Starting Listrak Order tracking ===');
            
            try {
                // Check if Listrak is loaded
                if (typeof _ltk !== 'undefined' && _ltk.Order) {
                    console.log('Listrak Order API found');
                    
                    // Set customer information if available
                    var customerEmail = getCookie('customerEmail') || '';
                    var customerFirstName = getCookie('customerFirstName') || '';
                    var customerLastName = getCookie('customerLastName') || '';
                    
                    if (customerEmail) {
                        _ltk.Order.SetCustomer(customerEmail, customerFirstName, customerLastName);
                        console.log('Customer set:', customerEmail, customerFirstName, customerLastName);
                    }
                    
                    // Set order totals
                    _ltk.Order.ItemTotal = response.itemTotal || 0;
                    _ltk.Order.ShippingTotal = response.shippingTotal || 0;
                    _ltk.Order.TaxTotal = response.taxTotal || 0;
                    _ltk.Order.HandlingTotal = response.handlingTotal || 0;
                    _ltk.Order.OrderTotal = response.orderTotal || 0;
                    
                    console.log('Order totals set:', {
                        itemTotal: _ltk.Order.ItemTotal,
                        shippingTotal: _ltk.Order.ShippingTotal,
                        taxTotal: _ltk.Order.TaxTotal,
                        handlingTotal: _ltk.Order.HandlingTotal,
                        orderTotal: _ltk.Order.OrderTotal
                    });
                    
                    // Add all cart items
                    if (response.cartItems && response.cartItems.length > 0) {
                        response.cartItems.forEach(function(item, index) {
                            console.log('Adding item ' + (index + 1) + ':', item);
                            _ltk.Order.AddItem(
                                item.sku || '',
                                item.quantity || 0,
                                item.price || 0
                            );
                        });
                    }
                    
                    // Submit the order
                    _ltk.Order.Submit();
                    console.log('Listrak Order submitted successfully');
                    
                } else {
                    console.error('Listrak Order API not found. Make sure Listrak script is loaded.');
                    console.log('Available _ltk object:', typeof _ltk !== 'undefined' ? _ltk : 'undefined');
                }
            } catch (listrakError) {
                console.error('Error in Listrak Order processing:', listrakError);
            }
        },
        error: function(xhr, status, error) {
            console.log('=== AJAX ERROR ===');
            console.error('Status:', status);
            console.error('Error:', error);
            console.error('Response Text:', xhr.responseText);
            
            alert('Error when adding item to cart.');
            cartSlideoutBackButton.style.display = "block";
            deliveryOptions.style.display = "block";
            addToCartButton.disabled = false;
        }
    });
    return false;
}