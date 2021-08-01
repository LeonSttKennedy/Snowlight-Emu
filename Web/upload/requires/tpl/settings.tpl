<h3>Account settings</h3>
<p>Update your preferencies!</p>
<br />
<form method="post" action="settings.php?pa=1">


<table width="100%" class="registration">

<?php
$query = mysql_query("SELECT * FROM characters WHERE username = '".$_SESSION['account_name']."'");
$get = mysql_fetch_assoc($query);
$reqs = $get['privacy_accept_friends'];
?>

<tr><td class="l" width="30%">
<strong>Friendship request</strong><p><small>Want to receive friend requests?</small></p></td>
<td class="r" width="100%"><input type="checkbox"  name="friendreqs" <?php if($reqs == "1") { echo "checked=\"on\""; } elseif($reqs == "0") { echo ""; } ?> />&nbsp;&nbsp;<?php if($reqs == "0") { echo "You are not receiving friend requests"; } else if($reqs == "1") { echo "You are receiving friend requests"; } ?><br /></td></tr>

<tr>
<td colspan="2" class="r" style="text-align: center;">
<br />
<br />
<tr><td colspan="2" class="r"><input type="submit" class="blbtn"; value="Update" /></td></tr></table>

</form>
