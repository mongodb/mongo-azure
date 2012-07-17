var fetched = false;

$(document).ready(function () {

    $("#logFetchStatus").fadeTo('fast', 1.0);
    refreshLogFile();
    $("a#refreshLog").click(refreshLogFile);
});

/**
* Fetches the log file from the server.
*/
function refreshLogFile() {

    if (fetched)
        $("#instanceLog").fadeTo('fast', 0.5);

    if ($("#logFetchError").is(':visible'))
        $("#logFetchError").fadeTo('fast', 0.5);

    $.ajax({ url: '/Server/GetServerLog/' + $("#server-id").text(), type: 'GET', success: function (response) {

        if (response.error) {
            $("#logFetchStatus").slideUp();
            $("#logFetchError").fadeIn();
            $("#logFetchError").fadeTo('fast', 1.0);
            $("#logFetchError .error-body").text(response.error);
        }
        else {
            $("#logFetchError").slideUp();
            $("#logFetchStatus").hide();
            $("#instanceLog").fadeIn();
            $("#instanceLog").fadeTo('fast', 1.0);
            $("#instanceLog").html(response.log);
            fetched = true;
        }
    }
    });

    return false;
}