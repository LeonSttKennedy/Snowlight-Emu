<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

if (isset($_POST['hatext']))
{
	fMessage('ok', 'Message sent:<br />"' . $_POST['hatext'] . '"');
	//$GetCore->Mus('ha', $_POST['hatext']);
}

require_once "top.php";

?>			

<h1>Hotel Alert</h1>

<br />

<p>
	Notify the entire hotel with an alert. Use with care. <i>Always double-check to avoid typos or errors.</i>
</p>

<br />

<p>
<?php if (isset($_POST['hatext'])) { ?>
<h1 style="padding: 15px;">Message Sent <span style="border: 2px dotted gray; padding: 10px; margin: 5px; font-size: 70%; font-weight: normal;"><?php echo $_POST['hatext']; ?></span><input type="button" value="Send new message?" onclick="document.location = 'index.php?_cmd=ha';"></h1>
<?php } else { ?>
<form method="post">

<textarea name="hatext" cols="30" rows="3"></textarea>
<input type="submit" value="Send">

</form>
</p>
<?php
}

require_once "bottom.php";

?>