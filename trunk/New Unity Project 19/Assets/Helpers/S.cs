using System;

public static class S
{
    public static string s4 = @"POST /login.php HTTP/1.0
Host: vkontakte.ru
Connection: Keep-Alive
User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
Referer: http://login.vk.com/
Content-Length: 138
Cache-Control: max-age=0
Origin: http://login.vk.com
Content-Type: application/x-www-form-urlencoded
Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
Cookie: remixchk=5; remixsid=nonenone

s=(passkey)&act=auth_result&m=4&permanent=&expire=1&app=1932732&app_hash=(apphash)";


    public static string s2 = @"POST / HTTP/1.0
Host: login.vk.com
Connection: Keep-Alive
User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
Referer: http://vkontakte.ru/login.php?app=1932732&layout=popup&type=browser
Content-Length: 129
Cache-Control: max-age=0
Origin: http://vkontakte.ru
Content-Type: application/x-www-form-urlencoded
Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3

act=login&app=1932732&app_hash=(apphash)&vk=&captcha_sid=&captcha_key=&email=(email)&pass=(pass)&expire=0&permanent=1";


    public static string s1 = @"GET /login.php?app=1932732&layout=popup&type=browser HTTP/1.1
Host: vkontakte.ru
Connection: keep-alive
User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
Cache-Control: max-age=0
Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
Accept-Encoding: gzip,deflate,sdch
Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
Cookie: remixchk=5; remixsid=nonenone

";

    public static string s5 = @"GET (url) HTTP/1.1
Host: api.vkontakte.ru
Connection: keep-alive
User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
Cache-Control: max-age=0
Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
Accept-Encoding: gzip,deflate,sdch
Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
Cookie: remixchk=5; remixsid=nonenone

";
}

