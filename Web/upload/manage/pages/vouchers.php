<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

if (isset($_GET['switch']) && is_numeric($_GET['switch']))
{
	$getvalue = mysql_result(mysql_query("SELECT enabled FROM vouchers WHERE id = '" . intval($_GET['switch']) . "' LIMIT 1"), 0);
	$newState = "1";

	if ($getvalue == "1")
	{
		$newState = "0";
	}
	
	mysql_query("UPDATE vouchers SET enabled = '" . $newState . "' WHERE id = '" . intval($_GET['switch']) . "' LIMIT 1");
	$getvalue = $newState;
	if($getvalue == "1")
	{
		fMessage('ok', "Voucher code is now enabled");
	}
	else
	{
		fMessage('ok', "Voucher code is disabled successfully");
	}
}

if (isset($_GET['del']) && is_numeric($_GET['del']))
{
	mysql_query("DELETE FROM vouchers WHERE id = '" . intval($_GET['del']) . "' LIMIT 1");
	fMessage('ok', "Voucher code is deleted successfully");
}

if (isset($_POST['v-code']))
{
	$vCode = $_POST['v-code'];
	$vCreditsValue = $_POST['v-cvalue'];
	$vPixelsValue = $_POST['v-pvalue'];
	$vFurniValue = $_POST['v-fvalue'];
	$vUses = $_POST['v-uses'];
	
	if (strlen($vCode) <= 0)
	{
		fMessage('error', 'Please enter a voucher code.');
	}
	else if (!is_numeric($vCreditsValue) || intval($vCreditsValue) <= 0 || intval($vCreditsValue) > 5000)
	{
		fMessage('error', 'Invalid credit value. Must be numeric and a value between 1 - 5000.');
	}
	else if (!is_numeric($vPixelsValue) || intval($vPixelsValue) <= 0 || intval($vPixelsValue) > 5000)
	{
		fMessage('error', 'Invalid pixel value. Must be numeric and a value between 1 - 5000.');
	}
	else if (!is_numeric($vUses) || intval($vUses) <= 0)
	{
		fMessage('error', 'Invalid uses value. Must be numeric and a value highter than 0.');
	}
	else
	{
		mysql_query("INSERT INTO vouchers (id, code, value_credits, value_pixels, value_furni, uses, enabled) VALUES (NULL, '" . $vCode . "', '" . $vCreditsValue . "', '" . $vPixelsValue . "', '" . $vFurniValue . "', '" . $vUses . "', '1')");
		fMessage('ok', 'Voucher is now live and redeemable.');
	}
}

require_once "top.php";

?>			

<h1>Vouchers</h1>

<br />

<p>
	Vouchers can be exchanged for credits, pixels or furnis on the website and in the ingame catalogue.
</p>

<br />

<p style="font-size: 125%; color: darkred;">
	<b>NOTE:</b> Staff are *NOT* to abuse this system. Vouchers may be used as a method of refunds,
	rewards, or prizes, but not to be handed out without VALID reason. Amounts must be kept reasonable.
	<u>Abuse of this system WILL be punished.</u>
</p>

<br />

<div style="float: left; width: 67%;">

	<h2>Redeemable vouchers</h2>
	
	<br />

	<table width="100%" border="1">
	<thead>
		<td>Voucher code</td>
		<td>Credits Amount</td>
		<td>Pixels Amount</td>
		<td>Furniture</td>
		<td>Uses remaining</td>
		<td>Enable/Disable</td>
		<td>Delete</td>
	</thead>
	<?php

	$get = mysql_query("SELECT * FROM vouchers ORDER BY enabled DESC");

	while ($user = mysql_fetch_assoc($get))
	{
		echo '<tr>';
		echo '<td>' . $user['code'] . '</td>';
		echo '<td>' . $user['value_credits'] . ' credits</td>';
		echo '<td>' . $user['value_pixels'] . ' pixels</td>';
		echo '<td>' . $user['value_furni'] . '</td>';
		echo '<td>' . $user['uses'] . '</td>';
		echo '<td><input type="button" value="'	. (($user['enabled'] == "1")? 'Disable' : 'Enable') . '" onclick="document.location = \'index.php?_cmd=vouchers&switch=' . $user['id'] . '\';"></td>';
		echo '<td><input type="button" value="Delete" onclick="document.location = \'index.php?_cmd=vouchers&del=' . $user['id'] . '\';"></td>';
		echo '</tr>';
	}

	?>
	</table>

</div>

<div style="float: right; width: 31%;">

	<h2>Add new voucher</h2>
	
	<br />
	
	<form method="post">
	
		Code:<br />
		<input type="text" name="v-code"><br />
		<br />
		Credit value:<br />
		<p style="font-size: 80%; color: darkred;">Credit value be between: 1 - 5000</p>
		<input type="text" name="v-cvalue"><br />
		<br />
		Pixel value:<br />
		<p style="font-size: 80%; color: darkred;">Pixels value be between: 1 - 5000</p>
		<input type="text" name="v-pvalue"><br />
		<br />
		Furni value:<br />
		<p style="font-size: 80%; color: darkred;">Can be used a '|' to separe multiple itens IDs.</p>
		<input type="text" name="v-fvalue"><br />
		<br />
		Uses:<br />
		<p style="font-size: 80%; color: darkred;">Enter here the number of times this code can be used.</p>
		<input type="text" name="v-uses"><br />
		<br />
		<input type="submit" value="Add">
	</form>

</div>

<div style="clear: both;"></div>

<?php

require_once "bottom.php";

?>