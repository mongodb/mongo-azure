$(document).ready(function () {
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