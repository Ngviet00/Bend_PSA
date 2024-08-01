export const STATUS_PLC = Object.freeze({
    'DISCONNECTED': 0,
    'START': 1,
    'STOP': 2,
    'ALARM': 3,
    'EMG': 4
});

export const STATUS_RESULT = Object.freeze({
    'OK': 1,
    'NG': 2,
    'EMPTY': 3,
});

export const SYSTEM_STATUS_CLIENT = Object.freeze({
    'RUNNING': 1,
    'PAUSE': 2,
    'ERROR': 3,
});

export const CLIENT = Object.freeze({
    'CLIENT_1': 1,
    'CLIENT_2': 2,
    'CLIENT_3': 3,
    'CLIENT_4': 4,
});

export const COLOR_STATUS = Object.freeze({
    'OK': '#5FB522',
    'NG': '#E4491D',
    'EMPTY': '#a7a7a7',
    'CHECKING': '#e9e413',
});