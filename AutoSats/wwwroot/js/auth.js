export function SignIn(password) {
    return new Promise((resolve, reject) => {
        var url = "/api/auth/login";
        var xhr = new XMLHttpRequest();

        // Initialization
        xhr.open("POST", url);
        xhr.setRequestHeader("Accept", "application/json");
        xhr.setRequestHeader("Content-Type", "application/json");

        // Catch response
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) // 4=DONE 
            {
                resolve(xhr.status === 200);
            }
        };

        // Data to send
        var data = {
            password: password
        };

        // Call API
        xhr.send(JSON.stringify(data));
    })
}