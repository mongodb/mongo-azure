var snapshotId = 0;

//=========================================================================
//
//  GENERAL
//
//=========================================================================

/**
* Runs when the document loads.
*/
$(document).ready(function () {

    getSnapshots();
    getBackups();

    // Hook up event handlers.
    $(".snapshot a.deleteSnapshot").live('click', deleteSnapshot_Click);
    $(".snapshot a.makeBackup").live('click', makeBackup_Click);
    $(".backup a.deleteBackup").live('click', deleteSnapshot_Click); // We actually reuse the code here.
});

//=========================================================================
//
//  SNAPSHOTS
//
//=========================================================================

/**
* Fetches the list of snapshots from the server.
*/
function getSnapshots() {

    $.ajax({ url: '/Snapshot/List', type: 'GET', success: function (response) {

        if (response.error) {
            alert("There was an error fetching the snapshots: " + response.error);
        }
        else {
            $.each(response.snapshots, function (i, snapshot) {
                $("#snapshotList").append("<li class='snapshot' id='snapshot_" + snapshotId + "'><span class='date'>" + snapshot.dateString + "</span> on <span class='blob'>" + snapshot.blob + "</span>"
                + " (<span class='snapshot-actions'><a class='makeBackup' href='#'>Make backup</a> | <a class='deleteSnapshot' href='#'>Delete</a></span>)</li>");
                $("#snapshot_" + snapshotId).data("uri", snapshot.uri);
                snapshotId++;

            });
            $("#snapshotFetchStatus").hide();
        }
    }
    });

    return false;
}

//=========================================================================
//
//  BACKUPS
//
//=========================================================================

/**
* Fetches the list of backups from the server.
*/
function getBackups() {

    $.ajax({ url: '/Backup/ListCompleted', type: 'GET', success: function (response) {

        if (response.error) {
            alert("There was an error fetching the backups: " + response.error);
        }
        else {
            $.each(response.snapshots, function (i, snapshot) {
                $("#backupList").append("<li class='backup' id='backup_" + snapshotId + "'><span class='name'>" + snapshot.name + "</span>"
                + " (<span class='backup-actions'><a class='deleteBackup' href='#'>Delete</a></span>)</li>");
                $("#backup_" + snapshotId).data("uri", snapshot.uri);
                snapshotId++;

            });
            $("#backupFetchStatus").hide();
        }
    }
    });

    return false;
}

//=========================================================================
//
//  EVENT HANDLERS
//
//=========================================================================

/**
* "Make backup" link on a snapshot clicked.
*/
function makeBackup_Click() {

    var item = $(this).closest('li');
    var uri = item.data("uri");

    $.ajax({ url: '/Backup/Start', data: { uri: uri }, type: 'POST', success: function (response) {
        $("#backupQueuedSuccess").fadeIn();
        $("#backupQueuedSuccess h4").text("The backup was started (job #" + response.jobId + ")");
    }
    });

    return false;
}

/**
* "Delete snapshot" on a snapshot clicked.
*/
function deleteSnapshot_Click() {

    var item = $(this).closest('li');
    var uri = item.data("uri");

    item.css({ "text-decoration": "line-through" });
    $.ajax({ url: '/Snapshot/Delete', data: { uri: uri }, type: 'POST', success: function (response) {
        item.slideUp();
    }
    });

    return false;
}