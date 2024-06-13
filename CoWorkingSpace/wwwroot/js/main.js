const dateInput = document.getElementById('date');
const startTimeInput = document.getElementById('startTime');
const endTimeInput = document.getElementById('endTime');
const priceInput = document.getElementById('price');

startTimeInput.addEventListener('input', updatePrice);
endTimeInput.addEventListener('input', updatePrice);

function updatePrice() {
    const startDate = new Date(dateInput.value);
    const startTime = parseTime(startTimeInput.value);
    const endTime = parseTime(endTimeInput.value);

    const price = calculatePrice(startDate, startTime, endTime);

    priceInput.value = price;
}

function parseTime(timeString) {
    const [hours, minutes] = timeString.split(':');
    return new Date(0, 0, 0, hours, minutes);
}

function calculatePrice(startDate, startTime, endTime) {
    const timeDiff = endTime - startTime;
    const hours = Math.floor(timeDiff / 1000 / 60 / 60);
    const price = hours * 5;

    if (isNaN(price) || !Number.isFinite(price)) {
        return "";
    }

    if (price < 0) {
        return "Enter a valid time";
    }

    return (`${price} BGN`);
}
