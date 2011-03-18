<div id="account-info"></div>
<button id="send-to-many">Send to Many</button>
<script>

function showAccountInfo() {
  FB.api(
    {
      method: 'fql.query',
      query: 'SELECT name, pic_square FROM user WHERE uid='+FB.getSession().uid
    },
    function(response) {
      document.getElementById('account-info').innerHTML = (
        '<img src="' + response[0].pic_square + '"> ' +
        response[0].name +
        ' <img onclick="FB.logout()" style="cursor: pointer;"' +
            'src="https://s-static.ak.fbcdn.net/rsrc.php/z2Y31/hash/cxrz4k7j.gif">'
      );
    }
  );
}

function showLoginButton() {
  document.getElementById('account-info').innerHTML = (
    '<img onclick="FB.login()" style="cursor: pointer;"' +
         'src="https://s-static.ak.fbcdn.net/rsrc.php/zB6N8/hash/4li2k73z.gif">'
  );
}

function onStatus(response) {
  Log.info('onStatus', response);
  if (response.session) {
    showAccountInfo();
  } else {
    showLoginButton();
  }
}
FB.getLoginStatus(function(response) {
  onStatus(response); // once on page load
  FB.Event.subscribe('auth.statusChange', onStatus); // every status change
});

document.getElementById('send-to-many').onclick = function() {
  FB.ui({
    method: 'apprequests',
    message: 'You should learn more about the @[19292868552:http://www.youtube.com/watch?v=LnYmmddTNrs].',
  }, null);
}
</script>
