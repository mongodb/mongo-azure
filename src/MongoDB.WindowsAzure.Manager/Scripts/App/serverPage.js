$(document).ready(function () {   

    setInterval( updateTable, 500 );
});

function updateTable() {
 $.ajax({
        url: "/api/server",
        contentType: "application/json;charset=utf-8",
        success: function (response) {

            $(".server-row").remove();
            $.each( response, addToTable );
        },
    });

}

function addToTable( index, server ) {

    $("#serversTable").append("<tr class='server-row'>" + 
        "<td>" + server.Id + "</td>" +
        "<td>" + server.Name + "</td>" +
        "<td>" + server.Health + "</td>" +
        "<td>" + server.CurrentState + "</td>" +
        "<td>" + server.LastHeartBeat + "</td>" +
        "<td>" + server.LastOperationTime + "</td>" +
        "<td>" + server.PingTime + "</td>" +
    "</tr>");

}