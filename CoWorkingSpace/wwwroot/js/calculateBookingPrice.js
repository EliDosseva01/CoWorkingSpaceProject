const startTimeInput = document.getElementById('startTime');
const endTimeInput = document.getElementById('endTime');
const date = document.getElementById("date")

startTimeInput.addEventListener('input', calculatePrice);
endTimeInput.addEventListener('input', calculatePrice);
date.addEventListener('input', calculatePrice);

async function CheckDayIsHoliday(currentDate) {
    let isHoliday = false;
    const apiUrl = 'https://openholidaysapi.org/';
    const dateFormatted = currentDate.toISOString().split('T')[0];

    const queryParams = `PublicHolidays?countryIsoCode=BG&languageIsoCode=BG&validFrom=${dateFormatted}&validTo=${dateFormatted}`;
    const requestUrl = apiUrl + queryParams;

    try {
        const response = await fetch(requestUrl);

        if (response.ok) {
            const responseContent = await response.json();
            console.log(responseContent);

            if (responseContent && responseContent.length > 0) {
                isHoliday = true;
            }
        } else {
            console.log('Request failed with status code: ' + response.status);
        }
    } catch (error) {
        console.error(error);
    }

    return isHoliday;
}

async function calculatePrice() {
    var startTime = document.getElementById("startTime").value;
    var endTime = document.getElementById("endTime").value;
    var currentDay = document.getElementById("date").value;


    var bookingDate = new Date(currentDay);
    var dayOfWeek = bookingDate.getDay();

    if (startTime && endTime) {
        var startHour = parseInt(startTime.split(":")[0]);
        var endHour = parseInt(endTime.split(":")[0]);

        var pricePerHour = 0;
        var isHoliday = await CheckDayIsHoliday(bookingDate);

        for (var hour = startHour; hour < endHour; hour++) {
            if (hour >= 9 && hour < 18 && dayOfWeek >= 1 && dayOfWeek <= 5 && !isHoliday) {
                pricePerHour += 5;
            } else {
                pricePerHour += 7;
            }
        }

        document.getElementById("price").value = `${pricePerHour} BGN`;
    }
}

