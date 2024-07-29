"use strict";

export function getCurrentDateTime() {
    return new Date(new Date().getTime() + (7 * 60 * 60 * 1000)).toISOString().slice(0, 23) + 'Z';
}

export function formatNumberWithDot(number) {
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}

export function convertDate(date) {
    let hours = date.substr(11, 2);
    let minutes = date.substr(14, 2);
    let seconds = date.substr(17, 2);
    let milliseconds = date.substr(20, 3);

    return hours + ":" + minutes + ":" + seconds + ":" + milliseconds;
}