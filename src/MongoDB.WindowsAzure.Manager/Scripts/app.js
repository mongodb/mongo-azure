$(document).ready(function () {


    $.ajax({ url: '/api/GetServerLogDirect/' + $("#server-id").text(), type: 'GET', success: function (response) {

        $("#logArea").fadeIn();
        $("#instanceLog").hide();
        $("#instanceLog").html(response.log);
        $("#instanceLog").fadeIn();
        $("#logFetchStatus").hide();
    }
    });

    $("#fetchLogButton").click(function () {

        $("#logArea").fadeIn();
        $.ajax({ url: '/api/GetServerLog/' + $("#server-id").text(), type: 'GET', success: function (response) {

            $("#instanceLog").hide();
            $("#instanceLog").html(response.log);
            $("#instanceLog").fadeIn();
            $("#logFetchStatus").hide();
        }
        });


    });
});