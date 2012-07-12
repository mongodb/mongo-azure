$(document).ready(function () {


    $.ajax({ url: '/api/GetServerLogDirect/' + $("#server-id").text(), type: 'GET', success: function (response) {
      
        $("#instanceLog").html(response.log);
    }
    });

    $("#fetchLogButton").click(function () {

        $("#logArea").fadeIn();
        $.ajax({ url: '/api/GetServerLogBlob/' + $("#server-id").text(), type: 'GET', success: function (response) {

            $("#instanceLog").hide();
            $("#instanceLog").html(response.log);
            $("#instanceLog").fadeIn();
            $("#logFetchStatus").hide();
        }
        });


    });
});