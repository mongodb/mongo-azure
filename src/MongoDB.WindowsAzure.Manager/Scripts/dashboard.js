var snapshotId = 0;

$(document).ready(function () {

    $("#logFetchStatus").fadeTo('fast', 0.8);
    getSnapshots();

    $(".snapshot a.deleteSnapshot").live('click', function () {
        var item = $(this).closest('li');
        var uri = item.data("uri");

        item.css({ "text-decoration": "line-through" });
        $.ajax({ url: '/Snapshot/Delete', data: { uri: uri }, type: 'POST', success: function (response) {
            item.slideUp();
        }});

        return false;
    });
});

/**
* Fetches the log file from the server.
*/
function getSnapshots() {

    if ($("#logFetchError").is(':visible'))
        $("#logFetchError").fadeTo('fast', 0.5);

    $.ajax({ url: '/Snapshot/GetAll', type: 'GET', success: function (response) {

        if (response.error) {
            $("#logFetchStatus").slideUp();
            $("#logFetchError").fadeIn();
            $("#logFetchError").fadeTo('fast', 1.0);
            $("#logFetchError .error-body").text(response.error);
        }
        else {
            $.each(response.snapshots, function (i, snapshot) {
                $("#snapshotList").append("<li class='snapshot' id='snapshot_" + snapshotId + "'><span class='date'>" + snapshot.dateString + "</span> on <span class='blob'>" + snapshot.blob + "</span>"
                + " (<span class='snapshot-actions'><a class='makeBackup' href='#'>Make backup</a> | <a class='deleteSnapshot' href='#'>Delete</a></span>)</li>");
                $("#snapshot_" + snapshotId).data("uri", snapshot.uri);
                snapshotId++;

            });
            $("#logFetchStatus").hide();
        }
    }
    });

    return false;
}