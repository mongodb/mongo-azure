$(document).ready(function () {

    $("#logFetchStatus").fadeTo('fast', 0.8);
    getSnapshots();    
});

/**
* Fetches the log file from the server.
*/
function getSnapshots() {

    if ($("#logFetchError").is(':visible'))
        $("#logFetchError").fadeTo('fast', 0.5);

    $.ajax({ url: '/Backup/GetSnapshots', type: 'GET', success: function (response) {

        if (response.error) {
            $("#logFetchStatus").slideUp();
            $("#logFetchError").fadeIn();
            $("#logFetchError").fadeTo('fast', 1.0);
            $("#logFetchError .error-body").text(response.error);
        }
        else {
            $.each(response.snapshots, function (i, snapshot) {
                $("#snapshotList").append("<li class='snapshot'><span class='date'>" + snapshot.dateString + "</span> on <span class='blob'>" + snapshot.blob + "</span>"
                + " (<span class='snapshot-actions'><a href='#'>Make backup</a> | <a href='#'>Delete</a></span>)</li>");

            });
            $("#logFetchStatus").hide();
        }
    }
    });

    return false;
}