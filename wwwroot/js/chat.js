"use strict";

var connection;
var subject;

function onReceiveMessage(user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user}: ${message}`;
}

function onClose(e) {
    createConnection();
}

function onConnectionStart() {
    document.getElementById("sendButton").disabled = false;

    // connect stream
    subject = new signalR.Subject();
    connection.send("SendMessageStream", subject);
}

function createConnection() {
    connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
    connection.on("ReceiveMessage", onReceiveMessage);

    connection.onclose(onClose);
    
    connection.start().then(onConnectionStart).catch(function (err) {
        return console.error(err.toString());
    });
}

createConnection();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    /* non-streaming
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    */
    console.log("send stream. message:" + message);
    var payload = {
        "user": user,
        "message": message
    };
    subject.next(JSON.stringify(payload));

    event.preventDefault();
});
