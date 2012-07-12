$(document).ready(function () {

    refreshLogFile();

    $("a#refreshLog").click(refreshLogFile); 
});

/**
 * Fetches the log file from the server.
 */
function refreshLogFile() {

    $("#instanceLog").fadeTo('fast', 0.5);
    $.ajax({ url: '/api/GetServerLogDirect/' + $("#server-id").text(), type: 'GET', success: function (response) {

        $("#logFetchStatus").hide();
        $("#instanceLog").fadeIn();
        $("#instanceLog").fadeTo('fast', 1.0);
        $("#instanceLog").html(response.log);
    }
    });
}