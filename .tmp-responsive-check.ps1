$ErrorActionPreference = 'Stop'
$origin = 'http://127.0.0.1:5174'
$debugPort = 9241
$profile = Join-Path $env:TEMP ('mis-responsive-' + [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds())
$chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
$auth = @{
  accessToken = 'layout-test-token'
  user = @{
    id = 'layout-test'; username = 'layout.test'; email = 'layout@example.invalid'; loginCode = 'LAYOUT'
    fullName = 'Responsive Layout Test'; department = 'HR'; role = 'Admin'; roles = @('Admin'); permissions = @()
  }
} | ConvertTo-Json -Depth 8 -Compress
$auth64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($auth))

$browser = Start-Process -FilePath $chrome -ArgumentList @('--headless=new','--disable-gpu','--disable-crash-reporter',"--remote-debugging-port=$debugPort","--user-data-dir=$profile",'--window-size=390,844',"$origin/login") -WindowStyle Hidden -PassThru
try {
  $version = $null
  for ($i = 0; $i -lt 50 -and -not $version; $i++) { Start-Sleep -Milliseconds 200; try { $version = Invoke-RestMethod "http://127.0.0.1:$debugPort/json/version" } catch {} }
  if (-not $version) { throw 'Chrome did not start.' }
  $tab = (Invoke-RestMethod "http://127.0.0.1:$debugPort/json") | Where-Object { $_.type -eq 'page' -and $_.url -like "$origin*" } | Select-Object -First 1
  $ws = [Net.WebSockets.ClientWebSocket]::new()
  $ws.ConnectAsync([Uri]$tab.webSocketDebuggerUrl, [Threading.CancellationToken]::None).GetAwaiter().GetResult() | Out-Null
  $script:id = 0
  function Cdp([string]$method, [hashtable]$params = @{}) {
    $script:id++; $id = $script:id
    $json = @{ id = $id; method = $method; params = $params } | ConvertTo-Json -Depth 30 -Compress
    $bytes = [Text.Encoding]::UTF8.GetBytes($json)
    $ws.SendAsync([ArraySegment[byte]]::new($bytes), [Net.WebSockets.WebSocketMessageType]::Text, $true, [Threading.CancellationToken]::None).GetAwaiter().GetResult() | Out-Null
    while ($true) {
      $stream = [IO.MemoryStream]::new()
      do {
        $buffer = New-Object byte[] 65536
        $received = $ws.ReceiveAsync([ArraySegment[byte]]::new($buffer), [Threading.CancellationToken]::None).GetAwaiter().GetResult()
        $stream.Write($buffer, 0, $received.Count)
      } while (-not $received.EndOfMessage)
      $message = [Text.Encoding]::UTF8.GetString($stream.ToArray()) | ConvertFrom-Json
      if ($message.id -eq $id) { return $message }
    }
  }
  function Js([string]$expression) {
    $response = Cdp 'Runtime.evaluate' @{ expression = $expression; returnByValue = $true; awaitPromise = $true }
    if ($response.result.exceptionDetails) { throw $response.result.exceptionDetails.exception.description }
    return $response.result.result.value
  }
  function WaitJs([string]$expression) {
    for ($i = 0; $i -lt 80; $i++) { Start-Sleep -Milliseconds 150; if (Js $expression) { return } }
    throw "Timed out: $expression"
  }

  WaitJs "document.readyState === 'complete'"
  Js "localStorage.setItem('mis.auth',atob('$auth64'));localStorage.setItem('mis.language','en');true" | Out-Null
  Cdp 'Page.navigate' @{ url = "$origin/hr/employees" } | Out-Null
  WaitJs "Boolean(document.querySelector('.module-sidebar')) && Boolean(document.querySelector('button[aria-expanded]'))"
  Start-Sleep -Milliseconds 500
  $closed = Js "(() => { const a=document.querySelector('.module-sidebar'),r=a.getBoundingClientRect(); return {viewport:[innerWidth,innerHeight],documentWidth:document.documentElement.scrollWidth,bodyOverflow:getComputedStyle(document.body).overflow,sidebar:{left:Math.round(r.left),right:Math.round(r.right),width:Math.round(r.width),hidden:a.getAttribute('aria-hidden'),inert:a.inert},mainWidth:Math.round(document.querySelector('.module-content').getBoundingClientRect().width)} })()"
  Js "document.querySelector('button[aria-expanded]').click();true" | Out-Null
  WaitJs "document.querySelector('button[aria-expanded]').getAttribute('aria-expanded') === 'true'"
  Start-Sleep -Milliseconds 250
  $open = Js "(() => { const a=document.querySelector('.module-sidebar'),r=a.getBoundingClientRect(),n=a.querySelector('nav'),f=a.querySelector('.module-sidebar-footer'); return {documentWidth:document.documentElement.scrollWidth,bodyOverflow:getComputedStyle(document.body).overflow,sidebar:{left:Math.round(r.left),right:Math.round(r.right),width:Math.round(r.width),height:Math.round(r.height),hidden:a.getAttribute('aria-hidden'),inert:a.inert},navigation:{clientHeight:n.clientHeight,scrollHeight:n.scrollHeight,overflow:getComputedStyle(n).overflowY},footer:{clientHeight:f.clientHeight,scrollHeight:f.scrollHeight,overflow:getComputedStyle(f).overflowY}} })()"
  Cdp 'Emulation.setDeviceMetricsOverride' @{ width = 667; height = 320; deviceScaleFactor = 1; mobile = $true } | Out-Null
  Start-Sleep -Milliseconds 300
  $landscape = Js "(() => { const a=document.querySelector('.module-sidebar'),n=a.querySelector('nav'),f=a.querySelector('.module-sidebar-footer'); return {viewport:[innerWidth,innerHeight],sidebarHeight:a.clientHeight,navigation:{clientHeight:n.clientHeight,scrollHeight:n.scrollHeight,scrollable:n.scrollHeight>n.clientHeight},footer:{clientHeight:f.clientHeight,scrollHeight:f.scrollHeight,scrollable:f.scrollHeight>f.clientHeight},documentWidth:document.documentElement.scrollWidth} })()"
  $tableFallback = Js "(() => { const host=document.createElement('section');host.className='overflow-hidden';const table=document.createElement('table');table.innerHTML='<tbody><tr><td>one</td><td>two</td></tr></tbody>';host.append(table);document.body.append(host);const result={hostOverflow:getComputedStyle(host).overflowX,tableMinWidth:getComputedStyle(table).minWidth};host.remove();return result })()"
  [pscustomobject]@{ closed = $closed; open = $open; landscape = $landscape; directTableFallback = $tableFallback } | ConvertTo-Json -Depth 10
  Cdp 'Browser.close' | Out-Null
  $ws.Dispose()
} finally {
  if (-not $browser.HasExited) { Stop-Process -Id $browser.Id -Force -ErrorAction SilentlyContinue }
  Start-Sleep -Milliseconds 200
  if (Test-Path -LiteralPath $profile) { Remove-Item -LiteralPath $profile -Recurse -Force }
}
