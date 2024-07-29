function updateCurrentTime() {
    let currentDate = new Date();
    let _currentDate = document.getElementById("date");
    let currentTime = document.getElementById("current-time");

    const options = {
        weekday: 'long',
        year: 'numeric',
        month: 'short',
        day: 'numeric',
    };

    //set current date
    _currentDate.innerHTML = currentDate.toLocaleString('en-US', options);

    currentDate = new Date();
    const hours = String(currentDate.getHours()).padStart(2, '0');
    const minutes = String(currentDate.getMinutes()).padStart(2, '0');
    const seconds = String(currentDate.getSeconds()).padStart(2, '0');
    const formatTime = `${hours}:${minutes}:${seconds}`;

    //set current time
    currentTime.innerHTML = formatTime;
}

updateCurrentTime();

setInterval(updateCurrentTime, 870);