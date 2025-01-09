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
    var str = `
        <div class="mobile-sidebar-title">
            <div class="back-button"> < Back </div>
            <a id="select_category_item" href="#" target="_blank">${categoryArray[0]}</a>
        </div>
    `;
    sidebar.append(str);

    header = $(".header-menu .mobile-sidebar-category.first");
    content = $(".header-menu .mobile-sidebar-category.second");
    selectCategory = $("#select_category_item");
    backButton = $(".mobile-sidebar-title .back-button");

    // Update link based on category name
    function updateCategoryLink(categoryName) {
        const baseURL = "https://stage.abcwarehouse.com/";
        const formattedName = categoryName.replace(/\s+/g, '-').toLowerCase(); // Replace spaces with hyphens and make lowercase
        selectCategory.attr("href", baseURL + formattedName);
    }

    // Set the initial link
    updateCategoryLink(categoryArray[0]);

    heightArray[index] = getHeight($(".mobile-sidebar-category.second > li"), "F");
    content.height(heightArray[index]);

    nextButton.click(function () {
        heightArray[index + 1] = getHeight($(this).next().find(".sublist:eq(0)>li"), "O");
        if (heightArray[index] > heightArray[index + 1]) {
            $(this).next().css("height", heightArray[index]);
            content.height(heightArray[index]);
        } else {
            $(this).next().css("height", heightArray[index + 1]);
            content.height(heightArray[index + 1]);
        }

        const newCategory = $(this).parent().find('a span').first().text();
        categoryArray[index + 1] = newCategory;
        updateCategoryLink(newCategory); // Update the link
        selectCategory.text(newCategory); // Update the displayed category name

        elementArray[index] = this;
        index++;
        sidebar.animate({ scrollTop: 0 }, 500);
        backButton.removeClass('hidden');
    });

    backButton.click(function () {
        if (index > 0) {
            index--;

            if (heightArray[index] > heightArray[index - 1]) {
                $(elementArray[index]).next().css("height", heightArray[index]);
                content.height(heightArray[index]);
            } else {
                $(elementArray[index]).next().css("height", heightArray[index - 1]);
                content.height(heightArray[index - 1]);
            }

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

    closeButton.click(function () {
        subContent.removeClass("active");
    });

    menuSetting();
    removeNonLeafLinks();

    $(".link-sub").click(function () {
        $(this).next().click();
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
