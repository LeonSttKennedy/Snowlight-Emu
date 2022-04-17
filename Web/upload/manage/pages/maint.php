<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$maint = mysql_fetch_array(mysql_query("SELECT * FROM site_config LIMIT 1"));
$maintenancestatus = $maint['maintenance'];
$maintenancealert = $maint['maintenance_text'];

if (isset($_GET['switch']))
{
	$maintenancetext = $_POST['mainttext'];
	$maintStatus = $_POST['maint'];
	if($maintStatus == 'on')
		$newmaintstatus = "1";
	elseif($maintStatus == '')
		$newmaintstatus = "0";
	
	$maintenancestatus =  $newmaintstatus;
	$maintenancealert = $maintenancetext;
	mysql_query("UPDATE site_config SET maintenance = '" . $newmaintstatus . "', maintenance_text = '" . $maintenancetext . "' LIMIT 1");
}

require_once "top.php";

?>			

<h1>Maintenance Mode</h1>

<br />

<p>
	 Maintenance mode can be used to disable the site and effectively prevent new logins to the server. Please note that any users still connected to the server or have a login session generated for them, will still be able to use the server until they are disconnected or it reboots.
</p>

<h2>Update Maintenance Status</h2><br />
<strong>Maintenace page text:</strong><br />
<form method="post" action="index.php?_cmd=maint&switch">
<input type="text" value="<?php echo $maintenancealert; ?>" name="mainttext" size="50" style="padding: 5px; font-size: 130%;"><br /><br />
<input type="checkbox"  name="maint" <?php if($maintenancestatus == "1") { echo "checked=\"on\""; } elseif($maintenancestatus == "0") { echo ""; } ?> />&nbsp;&nbsp;<?php if($maintenancestatus == "0") { echo "Maintenance mode is currently disabled."; } else if($maintenancestatus == "1") { echo "Maintenance mode is currently ENABLED. Site is not accessible to regular users."; } ?><br /><br />
<input type="submit" value="Update" style="color: darkred; font-weight: bold;">
</form>
<?php

require_once "bottom.php";

?>