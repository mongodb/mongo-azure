var nextId = 1;

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
    getBackupJobs();
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

    $.ajax({ url: '/Snapshot/List', cache: false, type: 'GET', success: function (response) {

        if (response.error) {
            alert("There was an error fetching the snapshots: " + response.error);
        }
        else {
            $("#snapshotList").empty();
            $.each(response.snapshots, function (i, snapshot) {
                var id = nextId++;

                $("#snapshotList").append("<li class='snapshot' id='snapshot_" + id + "'><span class='date'>" + snapshot.dateString + "</span> on <span class='blob'>" + snapshot.blob + "</span>"
                + " (<span class='snapshot-actions'><a class='makeBackup' href='#'>Make backup</a> | <a class='deleteSnapshot' href='#'>Delete</a></span>)</li>");
                $("#snapshot_" + id).data("uri", snapshot.uri);

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
* Fetches the list of backup jobs from the server.
*/
function getBackupJobs() {

    $.ajax({ url: '/Backup/ListJobs', cache: false, type: 'GET', success: function (response) {

        if (response.error) {
            alert("There was an error fetching the backup jobs: " + response.error);
        }
        else {
            $("#backupJobList").empty();
            $.each(response.jobs, function (i, job) {
                var id = nextId++;
                $("#backupJobList").append("<li class='job' id='job_" + id + "'><span class='name'>#" + job.id + "</span>: "
                + job.lastLine + " (<span class='backup-actions'><a href='Backup/ShowJob/" + job.id + "'>Details</a></span>)</li>");

            });
            $("#backupJobFetchStatus").hide();
        }
    }
    });

    return false;
}

/**
* Fetches the list of backups from the server.
*/
function getBackups() {

    $.ajax({ url: '/Backup/ListCompleted', cache: false, type: 'GET', success: function (response) {

        if (response.error) {
            alert("There was an error fetching the backups: " + response.error);
        }
        else {
            $("#backupList").empty();
            $.each(response.backups, function (i, backup) {
                var id = nextId++;
                $("#backupList").append("<li class='backup' id='backup_" + id + "'><span class='name'>" + backup.name + "</span>"
                + " (<span class='backup-actions'><a class='deleteBackup' href='#'>Delete</a></span>)</li>");
                $("#backup_" + id).data("uri", backup.uri);

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

    $.ajax({ url: '/Backup/Start', cache: false, data: { uri: uri }, type: 'POST', success: function (response) {
        $("#backupQueuedSuccess").fadeIn();
        $("#backupQueuedSuccess h4").text("The backup was started (job #" + response.jobId + ")");
        setTimeout(getBackupJobs, 300);
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
    $.ajax({ url: '/Snapshot/Delete', cache: false, data: { uri: uri }, type: 'POST', success: function (response) {
        item.slideUp();
    }
    });

    return false;
}