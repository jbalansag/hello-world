﻿var page = 0,
    inCallback = false,
    hasReachedEndOfInfiniteScroll = false;

var scrollHandler = function () {
    if (hasReachedEndOfInfiniteScroll == false &&
            //($("#propertyList").innerHeight() <= ($(window).scrollTop() + 470))) {
            //$(window).scrollTop() >= 900) {
            ($(window).scrollTop() == $(document).height() - $(window).height())) {
        loadMoreToInfiniteScrollDiv(moreRowsUrl);
    }
}

var ulScrollHandler = function () {

    if (hasReachedEndOfInfiniteScroll == false &&
            ($(window).scrollTop() == $(document).height() - $(window).height())) {
        loadMoreToInfiniteScrollUl(moreRowsUrl);
    }
}

function loadMoreToInfiniteScrollUl(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        $("div#loading").show();
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: "pageNum=" + page,
            success: function (data, textstatus) {
                if (data != '') {
                    $("ul.infinite-scroll").append(data);
                }
                else {
                    page = -1;
                }

                inCallback = false;
                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function loadMoreToInfiniteScrollDiv(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        $("div#loading").show();
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: "pageNum=" + page,
            success: function (data, textstatus) {
                if (data != '') {
                    $("div#propertyList").append(data);
                }
                else {
                    page = -1;
                    showNoMoreRecords();
                }

                inCallback = false;
                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function loadMoreToInfiniteScrollTable(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        $("div#loading").show();
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: "pageNum=" + page,
            success: function (data, textstatus) {
                if (data != '') {
                    $("table.infinite-scroll > tbody").append(data);
                    $("table.infinite-scroll > tbody > tr:even").addClass("alt-row-class");
                    $("table.infinite-scroll > tbody > tr:odd").removeClass("alt-row-class");
                }
                else {
                    page = -1;
                }

                inCallback = false;
                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function showNoMoreRecords() {
    hasReachedEndOfInfiniteScroll = true;
}