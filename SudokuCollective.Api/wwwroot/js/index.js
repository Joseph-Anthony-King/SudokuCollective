window.addEventListener('load', async () => {

    const date = new Date();

    try {

        let sudokuCollectiveIndexInfo;

        sudokuCollectiveIndexInfo = JSON.parse(localStorage.getItem('sudokuCollectiveIndexInfo'));

        if (!sudokuCollectiveIndexInfo || new Date(sudokuCollectiveIndexInfo.expirationDate) < date) {

            const response = await fetch('api/index');

            if (response.ok) {

                const missionStatement = (await response.json()).missionStatement;

                var expirationDate = new Date();

                expirationDate.setDate(expirationDate.getDate() + 1);

                sudokuCollectiveIndexInfo = { missionStatement, expirationDate }

                localStorage.setItem('sudokuCollectiveIndexInfo', JSON.stringify(sudokuCollectiveIndexInfo));

            } else {

                var data = JSON.parse(await response.text());

                data['status'] = response.status;

                console.debug('data: ', data);

                throw new Error(data.message);
            }
        }

        document.getElementById('missionStatement').innerHTML = sudokuCollectiveIndexInfo.missionStatement;

        document.getElementById('missionRow').classList.remove('hide');

    } catch (error) {
        
        console.error('Error returned: ', error);
    }
    
    document.getElementById('year').innerHTML = date.getFullYear();

    const htmlElement = document.getElementById('apiMessage');

    await checkAPIAsync(htmlElement);

    document.getElementById('spinner').classList.add('hide');

    document.getElementById('container').classList.remove('hide');

    document.getElementById('footer').classList.remove('hide');

    setInterval(async () => {

        await checkAPIAsync(htmlElement);

    }, 10000, htmlElement);

}, false);

async function checkAPIAsync(htmlElement) {

    try {
        
        const response = await fetch('api/v1/values', {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({
                page: 0,
                itemsPerPage: 0,
                sortBy: 0,
                orderByDescending: false,
                includeCompletedGames: false
            })
        });

        if (response.ok) {

            const data = await response.json();

            let message;

            if (data.isSuccess) {

                message = 'The Sudoku Collective API is up and running!';

            } else {

                message = data.message;
            }

            updateIndex(htmlElement, message, data.isSuccess);

        } else {

            var data = JSON.parse(await response.text());

            data['status'] = response.status;

            console.debug('data: ', data);

            if (data.status === 404 && data.message === 'Status Code 404: It was not possible to connect to the redis server(s). There was an authentication failure; check that passwords (or client certificates) are configured correctly: (IOException) Unable to read data from the transport connection: Connection aborted.') {
                console.debug("TODO: HerokuService reset redis connection logic will go here...")
            }

            throw new Error(data.message);
        }

    } catch (error) {
        
        updateIndex(
            htmlElement, 
            'Error connecting to Sudoku Collective API: <br/>' + error, 
            false);

        console.error('Error returned: ', error);
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

                htmlElement.classList.add('text-white');
                htmlElement.classList.remove('text-yellow');
            }
    
        } else {
    
            if (!htmlElement.classList.contains('text-yellow')) {

                htmlElement.classList.remove('text-white');
                htmlElement.classList.add('text-yellow');
            }
        }

    } else {

        if (typeof(message) !== 'string') {
            message = 'message invalid';
        }

        console.log('Invalid HTMLElement for message: ' + message);
    }
}
