<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

function SeasonalCurrencyEnuntoString($currency)
{
	switch($currency)
	{
		default:		
		case 0:
			$str_return = 'pixels';
			break;
				
		case 1:
			$str_return = 'snowflakes';
			break;
				
		case 2:
			$str_return = 'hearts';
			break;
							
		case 3:
			$str_return = 'giftpoints';
			break;
										
		case 4:
			$str_return = 'shells';
			break;
			
		case 5:
			$str_return = 'diamonds';
			break;
	}
	
	return $str_return;
}

function SeasonalCurrencyString($currency)
{
	switch($currency)
	{
		case "pixels":
			$str_return = 'Pixels';
			break;
				
		case "snowflakes":
			$str_return = 'Snowflakes';
			break;
				
		case "hearts":
			$str_return = 'Hearts';
			break;
							
		case "giftpoints":
			$str_return = 'Gift Points';
			break;
										
		case "shells":
			$str_return = 'Shells';
			break;
			
		case "diamonds":
			$str_return = 'Diamonds';
			break;
	}
	
	return $str_return;
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

if (isset($_GET['cr']) && is_numeric($_GET['cr']))
{
	$getvalue = mysql_result(mysql_query("SELECT can_reedem_in_catalog FROM vouchers WHERE id = '" . intval($_GET['cr']) . "' LIMIT 1"), 0);
	$newState = "1";

	if ($getvalue == "1")
	{
		$newState = "0";
	}
	
	mysql_query("UPDATE vouchers SET can_reedem_in_catalog = '" . $newState . "' WHERE id = '" . intval($_GET['cr']) . "' LIMIT 1");
	$getvalue = $newState;
	if($getvalue == "1")
	{
		fMessage('ok', "Voucher code now can reedem in catalogue");
	}
	else
	{
		fMessage('ok', "Voucher code catalogue reedem disabled successfully");
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
	$vCurrencyValue = $_POST['v-pvalue'];
	$vCurrencyType = $_POST['v-ctype'];
	$vFurniValue = $_POST['v-fvalue'];
	$vUses = $_POST['v-uses'];
	
	if (strlen($vCode) <= 0)
	{
		fMessage('error', 'Please enter a voucher code.');
	}
	else if (!is_numeric($vCreditsValue) || intval($vCreditsValue) < 0 || intval($vCreditsValue) > 5000)
	{
		fMessage('error', 'Invalid credit value. Must be numeric and a value between 0 - 5000.');
	}
	else if (!is_numeric($vCurrencyValue) || intval($vCurrencyValue) < 0 || intval($vCurrencyValue) > 5000)
	{
		fMessage('error', 'Invalid seasonal currency value. Must be numeric and a value between 0 - 5000.');
	}
	else if(!is_numeric($vCurrencyValue) || intval($vCurrencyType) < 0)
	{
		fMessage('error', 'Invalid seasonal currency type. Review your entry.');
	}
	else if (!is_numeric($vUses) || intval($vUses) <= 0)
	{
		fMessage('error', 'Invalid uses value. Must be numeric and a value highter than 0.');
	}
	else
	{
		mysql_query("INSERT INTO vouchers (id, code, value_credits, value_activity_points, seasonal_currency, value_furni, uses, enabled) VALUES (NULL, '" . $vCode . "', '" . $vCreditsValue . "', '" . $vCurrencyValue . "', '" . SeasonalCurrencyEnuntoString($vCurrencyType) . "', '" . $vFurniValue . "', '" . $vUses . "', '1')");
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
		<td>Currency Amount</td>
		<td>Furniture</td>
		<td>Uses remaining</td>
		<td>Catalogue reedem</td>
		<td>Enable/Disable</td>
		<td>Delete</td>
	</thead>
	<?php

	$get = mysql_query("SELECT * FROM vouchers ORDER BY enabled DESC");

	while ($voucher = mysql_fetch_assoc($get))
	{
		echo '<tr>';
		echo '<td>' . $voucher['code'] . '</td>';
		echo '<td>' . $voucher['value_credits'] . ' credits</td>';
		echo '<td>' . $voucher['value_activity_points'] . ' ' . SeasonalCurrencyString($voucher['seasonal_currency']) . '</td>';
		echo '<td>' . $voucher['value_furni'] . '</td>';
		echo '<td>' . $voucher['uses'] . '</td>';
		echo '<td><input type="button" value="'	. (($voucher['can_reedem_in_catalog'] == "1")? 'Disable' : 'Enable') . '" onclick="document.location = \'index.php?_cmd=vouchers&cr=' . $voucher['id'] . '\';"></td>';
		echo '<td><input type="button" value="'	. (($voucher['enabled'] == "1")? 'Disable' : 'Enable') . '" onclick="document.location = \'index.php?_cmd=vouchers&switch=' . $voucher['id'] . '\';"></td>';
		echo '<td><input type="button" value="Delete" onclick="document.location = \'index.php?_cmd=vouchers&del=' . $voucher['id'] . '\';"></td>';
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
		Seasonal currency value:<br />
		<p style="font-size: 80%; color: darkred;">Seasonal currency value be between: 0 - 5000</p>
		<input type="text" name="v-pvalue"><br />
		<br />
		Seasonal currency type:<br />
		<p style="font-size: 80%; color: darkred;">In case of voucher doesn't need activity points insert 0 in up text box</p>
		<select name="v-ctype">
			<option value="" disabled selected hidden>Without activity points</option>
			<option value="0">Pixels</option>
			<option value="1">Snowflakes</option>
			<option value="2">Hearts</option>
			<option value="3">Gift Points</option>
			<option value="4">Shells</option>
			<option value="5">Diamonds</option>
		</select><br />
		<br />
		Furni value:<br />
		<p style="font-size: 80%; color: darkred;">Can be used a '|' to separe multiple items IDs.</p>
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