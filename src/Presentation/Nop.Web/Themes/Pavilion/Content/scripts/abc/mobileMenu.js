var homePage = document.documentElement;
var bodyPage = document.getElementsByTagName("body")[0];
var sidePanel = $(".categories-in-side-panel");
var sidebar = $(".header-menu");
var header;
var content;
var subContent = $(".header-menu .sublist-wrap");
var selectCategory;
var nextButton = $(".header-menu .plus-button");
var backButton;
var closeButton = $(".header-menu .close-menu");

var index = 0;
var heightArray = new Array();
var imageArray = new Array('Default.png', 'HomeButton.png', 'LocationsButton.png', 'ClearanceTagButton.png', 'SaleAdButton.png', 'HawthStore.png', 'HawthWeeklyAD.png', 'HawthHome.png', 'BackButtonABCclearance.png', 'BackButtonHAWclearance.png', 'CreditIcon.png');
var element;
var elementArray = new Array();
var categoryArray = new Array();

categoryArray[0] = "SHOP ALL CATEGORIES";

$(document).ready(function () {
    var el = sidebar.find(".mega-menu-responsive");
    $(el[0]).addClass("mobile-sidebar-category first");
    $(el[1]).addClass("mobile-sidebar-category second");
    // var str = '<div class="mobile-sidebar-title"><div class="back-button"> < Back </div><div id="select_category_item">' + categoryArray[0] + '</div></div>';


    var str = `<div class="mobile-sidebar-title"><div class="back-button"> < Back </div><a id="select_category_item" href="#">${categoryArray[0]}</a></div>`;
    sidebar.append(str);

    header = $(".header-menu .mobile-sidebar-category.first");
    content = $(".header-menu .mobile-sidebar-category.second");
    selectCategory = $(".header-menu #select_category_item");
    backButton = $(".mobile-sidebar-title .back-button");

    effectBack();

    heightArray[index] = getHeight($(".mobile-sidebar-category.second > li"), "F");

    content.height(heightArray[index]);

    //Update Link based on category name, gonna remove stage if it works
    function updateCategoryLink(categoryName) {
        const baseURL = "https://abcwarehouse.com/";
        const baseURLHawthorne = "https://hawthorne.abcwarehouse.com/";

         // Check if we're on Hawthorne store by checking if createForHawthorne exists
    const isHawthorne = typeof createForHawthorne !== 'undefined' && 
    window.location.href.indexOf("hawthorne.abcwarehouse") > -1;

             // Special case for "shop-all-categories"

    if (categoryName.toLowerCase() === "shop all categories") {
        if (isHawthorne) {
            selectCategory.attr("href", baseURLHawthorne + "filterSearch");
            console.log("HAWTHORNE");
            
        } else {
            selectCategory.attr("href", baseURL + "filterSearch");
            console.log("ABC");
        }
        return;
    }

    
      // Special case for "dishwashers"
    else if (categoryName.toLowerCase() === "dishwashers") {
        selectCategory.attr("href", baseURL + "dishwashers-3");
        return;
    }
    
    // Special case for "Shop By Brand"
    else if (categoryName.toLowerCase() === "shop by brand") {
        selectCategory.attr("href", baseURL + "mattress-by-brand");
        return;
    }

    // Special case for "Blank Media /Usb Storage"
    else if (categoryName.toLowerCase() === "blank media/ usb storage") {
        selectCategory.attr("href", baseURL + "media-usb-storage");
        return;
    }
    //General case
    else
      {
         // const formattedName = categoryName.replace(/[\s/]+/g, '-').toLowerCase(); // Replace spaces with hyphens and make lowercase
         const formattedName = categoryName
         .replace(/[\s/&/-]+/g, '-') // Replace spaces, slashes, ampersands, and dashes with a single hyphen
         .replace(/[^a-z0-9-]/gi, '') // Remove any other non-alphanumeric characters except hyphens
         .toLowerCase(); // Convert to lowercase
         selectCategory.attr("href", baseURL + formattedName);
      }   
    }

  // Set the initial link
  updateCategoryLink(categoryArray[0]);



    nextButton.click(function () {
        heightArray[index + 1] = getHeight($(this).next().find(".sublist:eq(0)>li"), "O");
        if (heightArray[index] > heightArray[index + 1]) {
            $(this).next().css("height", heightArray[index]);
            content.height(heightArray[index]);
        } else {
            $(this).next().css("height", heightArray[index + 1]);
            content.height(heightArray[index + 1]);
        }

        backButton.removeClass('hidden');
        //selectCategory.text($(this).parent().find('a span').first().text());
        const newCategory = $(this).parent().find('a span').first().text();
        categoryArray[index + 1] = newCategory;
        updateCategoryLink(newCategory); // Update the link
        selectCategory.text(newCategory); // Update the displayed category name




        categoryArray[index + 1] = $(this).parent().find('a span').first().text();
        elementArray[index] = this;
        index++;
        sidebar.animate({ scrollTop: 0 }, 500);
    });


    backButton.click(function () {
        if (sidebar.find('.active').length > 0) {
            index--;

            if (heightArray[index] > heightArray[index - 1]) {
                $(elementArray[index]).next().css("height", heightArray[index]);
                content.height(heightArray[index]);
            } else {
                $(elementArray[index]).next().css("height", heightArray[index - 1]);
                content.height(heightArray[index - 1]);
            }

            // selectCategory.text(categoryArray[index]);
            const previousCategory = categoryArray[index];
            updateCategoryLink(previousCategory); // Update the link
            selectCategory.text(previousCategory); // Update the displayed category name


            if (sidebar.find('.active').length == 1) {
                content.height(heightArray[0]);
                sidebar.find('.sublist-wrap').removeClass('active');
            } else {
                $(elementArray[index]).next().removeClass('active');
            }
        }
        effectBack();
    });

    menuSetting();

    removeNonLeafLinks();

    $(".link-sub").click(function () {
        $(this).next().click();
    });

    closeButton.click(function () {
        subContent.removeClass("active");
    });
});

function toggleMobileMenu() {
    const isMobileMenuOpen = $(".categories-in-side-panel.open").length === 1;
    if (isMobileMenuOpen) {
        closeMobileMenu();
    } else {
        openMobileMenu();
    }
}

function openMobileMenu() {
    homePage.classList.add("scrollYRemove");
    bodyPage.classList.add("scrollYRemove");
    sidePanel.addClass("open");
    index = 0;
    heightArray[index] = getHeight($(".mobile-sidebar-category.second > li"), "F");
    var overlayCanvas = document.getElementsByClassName("overlayOffCanvas")[0];
    overlayCanvas.classList.add("show");
    overlayCanvas.style.display = "block";
    selectCategory.text(categoryArray[index]);
    backButton.addClass('hidden');
    sidebar.animate({ scrollTop: 0 }, 500);
    $('.overlayOffCanvas.show').on({ 'touchend': function () { closeButton.click(); } });
    sidebar.on({ 'touchstart': function () { $("html,body").scrollTop(50); } });
    sidebar.on({ 'touchmove': function () { $("html,body").scrollTop(50); } });
    sidebar.on({ 'touchend': function () { $("html,body").scrollTop(50); } });
}

function closeMobileMenu() {
    homePage.classList.remove("scrollYRemove");
    bodyPage.classList.remove("scrollYRemove");
    sidePanel.removeClass("open");
    var overlayCanvas = document.getElementsByClassName("overlayOffCanvas")[0];
    overlayCanvas.classList.remove("show");
    overlayCanvas.style.display = "none";
}

function effectBack() {
    if (sidebar.find('.active').length == 0) {
        backButton.addClass('hidden');
    } else {
        backButton.removeClass('hidden');
    }
}

// ABCTODO: Probably worth making a createAbc and using this method to
// coordinate between the three stores
function menuSetting() {
    var menu_array = header.find('li');
    var len = menu_array.length;
    var path = '';
    var storeFlag = "abc";
    for (var i = 0; i < len; i++) {
        if ($(menu_array[i]).find("a[href*='hawthorne']").length == 1) {
            storeFlag = "haw";
            break;
        }
    }

    for (var i = 0; i < len; i++) {
        const isAbc = storeFlag == "abc";
        var str = $(menu_array[i]).find('span').text().trim();
        var img = "";
        
        if (str == "Blog") {
            path = 'url(/Plugins/Misc.AbcFrontend/Images/BlogButton.png)';
        } else if (str == "Home") {
            if (storeFlag == "abc") {
                path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[1] + ')';
            } else {
                path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[7] + ')';
            }
        } else if (str == "Home Page") {
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[7] + ')';
        } else if (str == "Locations") {
            if (storeFlag == "abc") {
                path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[2] + ')';
            } else {
                path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[5] + ')';
            }
        } else if (str == "Clearance") {
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[3] + ')';
        } else if (str == "Sale Ad") {
            if (storeFlag == "abc") {
                path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[4] + ')';
            } else {
                path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[6] + ')';
            }
        } else if (str == "Store Locations") {
            img = isAbc ? imageArray[2] : imageArray[5];
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + img + ')';
        } else if (str == "Financing") {
            img = imageArray[10];
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + img + ')';
        } else if (str == "Weekly Ad") {
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[6] + ')';
        } else if (str.indexOf("Back") !== -1) {
            img = isAbc ? imageArray[8] : imageArray[9];
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + img + ')';
        } else {
            path = 'url(/Plugins/Misc.AbcFrontend/Images/' + imageArray[0] + ')';
        }
        $(menu_array[i]).find('a').css('background-image', path);
        $(menu_array[i]).css('width', '33.3%');
    }
    $(menu_array[len]).append("<div class='phone-line'></div>");
    $(menu_array[len]).find('span').css('line-height', '0px');

    createForHawthorne(menu_array, len);
    createForMickeyShorr(menu_array, len);
}

function createForHawthorne(menu_array, len) {
    // hardcode this to true for local testing
    const isHawthorne = window.location.href.indexOf("hawthorneonline") > -1;
    if (!isHawthorne) { return; }

    // adjust sidebar title for single row of icons
    var mobileSidebarTitle = document.getElementsByClassName("mobile-sidebar-title")[0];
    mobileSidebarTitle.style.top = "125px";

    $(menu_array[0]).css('width', "25%");
    $(menu_array[0]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/LocationsButton.png')");
    $(menu_array[0]).find('a').attr("href", '/AllShops');
    $(menu_array[0]).find('a').find('span').text('Locations');
    $(menu_array[1]).css('width', "25%");
    $(menu_array[1]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/CreditIcon.png')");
    $(menu_array[1]).find('a').attr("href", '/hawthorne-credit-card');
    $(menu_array[1]).find('a').find('span').text('Financing');
    $(menu_array[2]).css('width', "25%");
    $(menu_array[2]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/ClearanceTagButton.png')");
    $(menu_array[2]).find('a').attr("href", 'https://clearance.hawthorneonline.com');
    $(menu_array[2]).find('a').find('span').text('Clearance');
    $(menu_array[3]).css('width', "25%");
    $(menu_array[3]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/SaleAdButton.png')");
    $(menu_array[3]).find('a').attr("href", '/special-financing-options-2');
    $(menu_array[3]).find('a').find('span').text('Lookbook');

    for (var i = 4; i < len; i++)
    {
        $(menu_array[i]).remove();
    }
}

function createForMickeyShorr(menu_array, len) {
    // hardcode this to true for local testing
    const isMickeyShorr = window.location.href.indexOf("mickeyshorr") > -1;
    if (!isMickeyShorr) { return; }

    // adjust sidebar title for single row of icons
    var mobileSidebarTitle = document.getElementsByClassName("mobile-sidebar-title")[0];
    mobileSidebarTitle.style.top = "125px";

    $(menu_array[0]).css('width', "33.3%");
    $(menu_array[0]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/LocationsButton.png')");
    $(menu_array[0]).find('a').attr("href", '/AllShops')
    $(menu_array[0]).find('a').find('span').text('Locations')
    $(menu_array[1]).css('width', "33.3%");
    $(menu_array[1]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/SaleAdButton.png')");
    $(menu_array[1]).find('a').attr("href", '/sale-ad')
    $(menu_array[1]).find('a').find('span').text('Sale Ad')
    $(menu_array[2]).css('width', "33.3%");
    $(menu_array[2]).find('a').css('background-image', "url('/Plugins/Misc.AbcFrontend/Images/CreditIcon.png')");
    $(menu_array[2]).find('a').attr("href", '/special-financing-options-2')
    $(menu_array[2]).find('a').find('span').text('Financing')

    for (var i = 3; i < len; i++)
    {
        $(menu_array[i]).remove();
    }
}

function removeNonLeafLinks() {
    var links = sidebar.find(".mobile-sidebar-category .has-sublist");
    for (var j = 0; j < links.length; j++) {
        $(links[j]).find("a:eq(0)").removeAttr("href");
        $(links[j]).find("a:eq(0)").addClass("link-sub");
    }
}

function getHeight(el, flag) {
    var h = 0;
    if (flag == "F") {
        for (var i = 0; i < el.length; i++) {
            h += $(el[i]).height();
        }
    } else if (flag == "O") {
        for (var i = 1; i < el.length; i++) {
            h += $(el[i]).height();
        }
    }
    return h;
}
