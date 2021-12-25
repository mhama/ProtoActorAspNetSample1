"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

const subject = new signalR.Subject();


//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;

    // connect stream
    connection.send("SendMessageStream", subject);
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    /* non-streaming
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    */
    console.log("send stream. message:" + message);
    subject.next("streaming: " + user + ": " + message);

    event.preventDefault();
});