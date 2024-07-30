"use strict";

import { STATUS_PLC, SYSTEM_STATUS_CLIENT, STATUS_RESULT, CLIENT, COLOR_STATUS } from "./const.js";

import { getCurrentDateTime, formatNumberWithDot, convertDate } from "./common.js";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/homeHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

$(function () {
    var timeouts = [null, null, null, null];
    var deepcores = [null, null, null, null];
    var clientConnects = [null, null, null, null];
    var previousTray = [];
    var resetPLC = 1;

    const queue = [];
    let processing = false;
    let checkHaveCheking = false;
    let currentIndexRoll = 0;

    const noDataTimeLogRow = $('.time-log-no-data');

    var statusPLC = null;
    var statusClient = null;

    var ConnectPC = [null, null, null, null]
    var CamPC = [null, null, null, null]
    var DeepLearningPC = [null, null, null, null]


    for (let i = 1; i <= 4; i++) {
        clearTimeout(timeouts[i]);
        clearTimeout(deepcores[i]);
        clearTimeout(clientConnects[i]);
    }

    //====================================================== EVENT REALTIME ======================================================

    //connection start
    connection.start()
        .then(() => {
            console.log('Connection established!');
        })
        .catch((err) => {
            console.error(err.toString())
        });

    connection.on("ShowTotalQtyToScreen", (total, ok, ng, empty) => {

        let percentOK = total == 0 ? 0 : parseFloat((ok / total * 100).toFixed(2))
        let percentNG = total == 0 ? 0 : parseFloat((ng / total * 100).toFixed(2))
        let percentEmpty = parseFloat((100 - percentOK - percentNG).toFixed(2))

        $('#total-ea').html(`${formatNumberWithDot(total)}<span class="">&nbspEA</span>`);
        $('#total-ok-ea').html(`${formatNumberWithDot(ok)}<span class="">&nbspEA</span>`);
        $('#total-ng-ea').html(`${formatNumberWithDot(ng)}<span class="">&nbspEA</span>`);
        $('#total-empty-ea').html(`${formatNumberWithDot(empty)}<span class="">&nbspEA</span>`);

        $('#percent-ok').html(`${percentOK} %`);
        $('#percent-ng').html(`${percentNG} %`);
        $('#percent-empty').html(`${percentEmpty} %`);

        if (percentOK == 0 && percentNG == 0 && percentEmpty == 0) {
            percentOK = 100;
        }

        myPieChart.data.datasets[0].data = [percentOK, percentNG, percentEmpty];
        myPieChart.data.labels = ["OK", "NG", "Empty"];
        myPieChart.update('none');
    })

    connection.on("SendDataToClient", (data1, data2, model, currentRoll) => {
        if (checkHaveCheking) {
            let valueItemCurrentLeft = $('.current-item .left').html();
            let valueItemCurrentRight = $('.current-item .right').html();

            $('.previous-item #previous-item-not-data').remove();

            currentIndexRoll += 1;

            let result = `
                <div style="text-align:left">${currentIndexRoll}</div>
                <div class="d-flex mt-1">
                    <div class="left ${valueItemCurrentLeft == 'OK' ? 'ok' : 'ng'}">${valueItemCurrentLeft == 'OK' ? 'NG' : 'NG'}</div>
                    <div class="right ${valueItemCurrentRight == 'OK' ? 'ok' : 'ng'}">${valueItemCurrentRight == 'OK' ? 'NG' : 'NG'}</div>
                </div>
            `;

            $('.previous-item .container').prepend(result);
        }

        queue.push({ data1, data2 });
        processQueue(model, currentRoll);
    });

    function processQueue(model, currentRoll) {
        if (queue.length === 0 || processing) {
            return;
        }

        processing = true;
        checkHaveCheking = true;

        const { data1, data2 } = queue.shift();

        let currentItemLeft = $('.current-item .left');
        let currentItemRight = $('.current-item .right');

        currentItemLeft.text('Checking...');
        currentItemLeft.css('background', COLOR_STATUS.CHECKING);

        currentItemRight.text('Checking...');
        currentItemRight.css('background', COLOR_STATUS.CHECKING);

        setTimeout(() => {
            currentItemLeft.text(data1.result === STATUS_RESULT.OK ? 'OK' : 'NG');
            currentItemLeft.css('background', data1.result === STATUS_RESULT.OK ? COLOR_STATUS.OK : COLOR_STATUS.NG);

            currentItemRight.text(data2.result === STATUS_RESULT.OK ? 'OK' : 'NG');
            currentItemRight.css('background', data2.result === STATUS_RESULT.OK ? COLOR_STATUS.OK : COLOR_STATUS.NG);

            appendResultLog(data1, model, currentRoll);
            appendResultLog(data2, model, currentRoll);

            processing = false;
            processQueue(model, currentRoll);
        }, 150);
    }

    //====================================================== CONFIG CHART ======================================================
    var ctx = document.getElementById('pie-chart').getContext('2d');

    var values = [
        percentOK,
        percentNG,
        percentEmpty
    ];

    if (percentOK == 0 && percentNG == 0 && percentEmpty == 0) {
        values = [100, 0, 0]
    }

    var myPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ["OK", "NG", "Empty"],
            datasets: [{
                data: values,
                backgroundColor: [
                    '#66b032', '#e4491d', '#9F9F9F',
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: false,
                text: null,
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        color: '#667085',
                    }
                },
                tooltip: {
                    enabled: false,
                },
                datalabels: {
                    formatter: (value, context) => {
                        const datapoints = context.chart.data.datasets[0].data;
                        function totalSum(total, datapoint) {
                            return total + datapoint;
                        }
                        const totalValue = datapoints.reduce(totalSum, 0);
                        const percentageValue = (value / totalValue * 100).toFixed(2);
                        return `${percentageValue}%`;
                    },
                    color: '#ffffff',
                    font: {
                        weight: 'bold',
                        size: 15,
                    },
                    align: 'end',
                    anchor: 'center'
                }
            }
        },
        plugins: [ChartDataLabels]
    });

    //====================================================== FUNCTION ======================================================
    function appendTimeLog(time, type, message) {
        if (noDataTimeLogRow) {
            noDataTimeLogRow.remove();
        }

        $("#time-log table tbody").prepend(`
            <tr>
                <td class="max-w90">${convertDate(time)}</td>
                <td class="max-w105">${type}</td>
                <td>${message}</td>
            </tr>
        `);
    }

    function appendResultLog(data, model, currentRoll) {
        $("#result-log table tbody").prepend(`
            <tr>
                <td>${convertDate(data.time)}</td >
                <td class="text-capitalize">${model == '' ? 'Stiff_953' : model}</td>
                <td>${data.clientId}</td>
                <td>${currentRoll}</td>
                <td>${result == STATUS_RESULT.OK ? 'OK' : 'NG'}</td>
                <td>1</td>
            </tr>
        `);
    }

    //====================================================== Form search and load more ======================================================
    /*var pageListResult = 1;
    var totalListResult = 0;
    var totalPage = 0;

    $('.form-search-btn-search').click(function () {
        $('.form-search-btn-search').prop('disabled', true).html('Loading...');
        let fromDate = $('#start-date').val() + ' ' + $('#start-time').val();
        let toDate = $('#end-date').val() + ' ' + $('#end-time').val();
        let model = $('#form-search-model').val();
        pageListResult = 1;

        connection.invoke("SearchData", fromDate, toDate, pageListResult, model)
            .then(function (res) {
                $('#form-search-total-tray-ea').html(formatNumberWithDot(res.totalTray));
                $('#form-search-total-ea').html(`${formatNumberWithDot(res.total)}<span class="">&nbspEA</span>`);
                $('#form-search-total-ok-ea').html(`${formatNumberWithDot(res.totalOK)}<span class= ""> EA</span>`);
                $('#form-search-total-ng-ea').html(`${formatNumberWithDot(res.totalNG)}<span class="">EA</span>`);
                $('#form-search-total-empty-ea').html(`${formatNumberWithDot(res.totalEmpty)}<span class= "" > EA</span >`);
                $('#form-search-percent-ok').html(`${res.percentOK} %`);
                $('#form-search-percent-ng').html(`${res.percentNG} %`);
                $('#form-search-percent-empty').html(`${res.percentEmpty} %`);

                totalListResult = res.total;

                totalPage = Math.ceil(totalListResult / 20);


                if (totalListResult > 40) {
                    $('.form-search-btn-load-more').prop('disabled', false);
                }

                let data = '';
                if (res.results.length > 0) {
                    res.results.forEach(item => {
                        let result = GetResult(item);

                        let err = '';
                        item.errors.forEach(itemErr => {
                            err += ',' + itemErr.description;
                        });

                        let img = '';
                        item.images.forEach(itemImg => {
                            if (itemImg.path.trim() != 'No_save' && itemImg.path.trim() != '') {
                                img += `<a target="_blank" href="${convertPathToUrl(itemImg.path, getUrlBase(itemImg.clientId))}">Image</a>,`;
                            }
                        });

                        data += `
                            <tr style="font-size: 14px; font-weight: 500;">
                                <td>${item.id}</td>
                                <td style="width: 200px">${item.time}</td>
                                <td style="width: 200px">${item.model}</td>
                                <td>${item.tray}</td>
                                <td>${item.index}</td>
                                <td class="${result == 'NG' ? 'text-danger' : 'text-success'}">${result}</td>
                                <td>${result != 'NG' ? '-' : err.replace(/^,+|,+$/g, '')}</td>
                                <td>
                                    ${result != 'NG' ? '-' : img.replace(/,+$/, '') }
                                </td>
                            </tr>
                        `;
                    });

                    $('#action-form-search #list-result tbody').html('').append(data);
                } else {
                    $('#action-form-search #list-result tbody').html('').append(
                        `<tr>
                            <td colspan="12" class="text-center fw-bold text-dark tag-notice">Not Found Data</td>
                        </tr>`
                    );
                }
            })
            .catch(function (err) {
                console.error("Error calling API:", err.toString());
            })
            .finally(function () {
                $('.form-search-btn-search').prop('disabled', false).html('Search');
            });
    });

    $('.form-search-btn-load-more').click(function () {
        $(this).prop('disabled', true).html('Loading...');

        pageListResult++;

        if (pageListResult == totalPage) {
            $(this).prop('disabled', true);
        }

        let fromDate = $('#start-date').val() + ' ' + $('#start-time').val();
        let toDate = $('#end-date').val() + ' ' + $('#end-time').val();
        let model = $('#form-search-model').val();

        connection.invoke("SearchData", fromDate, toDate, pageListResult, model)
            .then(function (res) {

                let data = '';
                if (res.results.length > 0) {
                    res.results.forEach(item => {
                        let result = GetResult(item);

                        let err = '';
                        item.errors.forEach(itemErr => {
                            err += ',' + itemErr.description;
                        });

                        let img = '';
                        item.images.forEach(itemImg => {
                            if (itemImg.path.trim() != 'No_save' && itemImg.path.trim() != '') {
                                img += `<a target="_blank" href="${convertPathToUrl(itemImg.path, getUrlBase(itemImg.clientId))}">Image</a>,`;
                            }
                        });

                        data += `
                            <tr style="font-size: 14px; font-weight: 500;">
                                <td>${item.id}</td>
                                <td style="width: 200px">${item.time}</td>
                                <td style="width: 200px">${item.model}</td>
                                <td>${item.tray}</td>
                                <td>${item.index}</td>
                                <td class="${result == 'NG' ? 'text-danger' : 'text-success'}">${result}</td>
                                <td>${result != 'NG' ? '-' : err.replace(/^,+|,+$/g, '')}</td>
                                <td>
                                    ${result != 'NG' ? '-' : img.replace(/,+$/, '') }
                                </td>
                            </tr>
                        `;
                    });

                    $('#action-form-search #list-result tbody').append(data);
                }
            })
            .catch(function (err) {
                console.error("Error calling API:", err.toString());
            })
            .finally(function () {
                $('.form-search-btn-load-more').prop('disabled', false).html('Load more');
            });
    });*/
});