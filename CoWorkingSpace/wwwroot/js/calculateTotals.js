function calculateTotals() {

    let arrOfPrices = Array.from(document.querySelectorAll('.price'));
    let parsedPrices = arrOfPrices.map(price => price.textContent.slice(0, -7));

    let totalPrice = 0;
    parsedPrices.forEach(price => totalPrice += Number(price));
    document.querySelector('.total-price').textContent = `Total price: ${totalPrice} BGN`;

    let arrOfStartTimes = Array.from(document.querySelectorAll('.startTime'));
    let parsedArrStartTime = arrOfStartTimes.map(time => time.textContent.slice(0, 2));

    let arrOfEndTimes = Array.from(document.querySelectorAll('.endTime'));
    let parsedArrEndTime = arrOfEndTimes.map(time => time.textContent.slice(0, 2));

    let totalHours = 0;
    for (let i = 0; i < parsedArrEndTime.length; i++) {

        totalHours += (Number(parsedArrEndTime[i]) - Number(parsedArrStartTime[i]));
    }

    document.querySelector('.total-hours').textContent = `Total hours: ${totalHours} hours`;
}
