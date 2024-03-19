<div class="column1" id="column1">
<div class="module texthtml">
<style type="text/css">
.charentry { 
width: 100%; 
background-color: #fff; 
border-radius: 4px; 
border: 2px solid #F2F2F2; 
margin-bottom: 5px; 
}
.charentry .inner { 
padding: 4px 10px 6px 10px; 
}
</style>
<div class="content texthtml">
<div class="charentry">
<div class="inner">
<div class="left" style="width: 65%;">
<img src="https://imager.habboon.pw/?size=b&action=wav&figure=<?php GetUserInfo($_SESSION['id'], "figure"); ?>" alt="Avatar" align="left" />
<p style="margin-top: 25px; margin-left: 5px; font-size: 125%;">
<strong><?php GetUserInfo($_SESSION['id'], "username"); ?></strong>
<br />
<em>"<?php GetUserInfo($_SESSION['id'], "motto"); ?>"</em>
</p>
</div>
<div class="right" style="margin-top: 20px; width: 35%; font-size: 110%;">
<a style="font-size: 150%;" href="client.php" onclick="window.open('client.php', 'client', 'width=1100,height=700,location=false,status=false,menubar=false,directories=false,toolbar=false,resizable=true,scrollbars=false'); return false;">Enter &raquo;</a>
<p><img src="http://<?php echo SITE_DOMAIN; ?>/images/icon_credits.png" style="margin-right: 5px;" /><?php GetUserInfo($_SESSION['id'], "credits_balance"); ?>&nbsp;&nbsp;<img src="http://<?php echo SITE_DOMAIN; ?>/images/icon_ap.png" style="margin-left: 5px; margin-right: 5px;" /><?php Users::GetActivityPointsValue($_SESSION['id'], 0); ?></p>
</div>
<div class="clear">
</div>
</div>
</div>
</div>
</div>
<?php Texts("frontpage"); ?>
</div>

<div class="column2" id="column2">
<div class="module texthtml"><div class="content texthtml">
  <div>

    <input type="hidden" name="cx" value="partner-pub-3019110740111649:3599668358" />

    <input type="hidden" name="ie" value="UTF-8" />

    <input type="text" name="q" size="44" />

  </div>

</form>



<script type="text/javascript" src="http://www.google.com/coop/cse/brand?form=cse-search-box&amp;lang=en"></script>



<script type="text/javascript" src="http://www.google.com/cse/query_renderer.js"></script>

<div id="queries"></div>

<script src="http://www.google.com/cse/api/partner-pub-3019110740111649/cse/3599668358/queries/js?oe=UTF-8&amp;callback=(new+PopularQueryRenderer(document.getElementById(%22queries%22))).render"></script></div></div><div class="module newslist">

<?php include("./requires/tpl/news_articles.tpl"); ?>

		</div><div class="module texthtml"><div class="content texthtml"><div style="text-align: center;">
</div></div></div>
