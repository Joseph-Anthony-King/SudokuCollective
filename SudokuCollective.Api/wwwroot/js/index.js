window.addEventListener('load', async () => {

    const date = new Date();

    try {

        let sudokuCollectiveIndexInfo;

        sudokuCollectiveIndexInfo = JSON.parse(localStorage.getItem('sudokuCollectiveIndexInfo'));

        if (!sudokuCollectiveIndexInfo || new Date(sudokuCollectiveIndexInfo.expirationDate) < date) {

            const response = await fetch("api/index");
        
            const missionStatement = (await response.json()).missionStatement;
            
            var expirationDate = new Date();
            
            expirationDate.setDate(expirationDate.getDate() + 1);
        
            sudokuCollectiveIndexInfo = { missionStatement, expirationDate }
        
            localStorage.setItem('sudokuCollectiveIndexInfo', JSON.stringify(sudokuCollectiveIndexInfo));
        }

        document.getElementById('missionStatement').innerHTML = sudokuCollectiveIndexInfo.missionStatement;

        document.getElementById('missionRow').classList.remove('hide');

    } catch (error) {
        
        console.log(error);
    }
    
    document.getElementById('year').innerHTML = date.getFullYear();

    const htmlElement = document.getElementById('apiMessage');

    await checkAPI(htmlElement);

    document.getElementById('spinner').classList.add('hide');

    document.getElementById('container').classList.remove('hide');

    document.getElementById('footer').classList.remove('hide');

    setInterval(async () => {

        await checkAPI(htmlElement);

    }, 10000, htmlElement);

}, false);

async function checkAPI(htmlElement) {

    try {
        
        const response = await fetch("api/helloworld");

        const data = await response.json();

        let message;

        if (data.isSuccess) {

            message = 'The Sudoku Collective API is up and running!';

        } else {

            message = data.message;
        }

        updateIndex(htmlElement, message, data.isSuccess);

    } catch (error) {
        
        updateIndex(
            htmlElement, 
            'Error connecting to Sudoku Collective API: <br/>' + error, 
            false);
    }
}

function updateIndex(htmlElement, message, isSuccess) {

    if (htmlElement.innerHTML !== undefined 
        && htmlElement.classList !== undefined 
        && typeof(message) === 'string' 
        && typeof(isSuccess) === 'boolean') {

        htmlElement.innerHTML = message;

        if (isSuccess) {
    
            if (htmlElement.classList.contains('text-yellow')) {

                htmlElement.classList.add("text-white");
                htmlElement.classList.remove("text-yellow");
            }
    
        } else {
    
            if (!htmlElement.classList.contains('text-yellow')) {

                htmlElement.classList.remove("text-white");
                htmlElement.classList.add("text-yellow");
            }
        }

    } else {

        if (typeof(message) !== 'string') {
            message = 'message invalid';
        }

        console.log("Invalid HTMLElement for message: " + message);
    }
}
