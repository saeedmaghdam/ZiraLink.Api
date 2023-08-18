;
; BIND reverse data file for "this host on this network" zone
;
$TTL    604800
@       IN      SOA     ns1.ziralink.com. ns2.ziralink.com. (
                              1         ; Serial
                         604800         ; Refresh
                          86400         ; Retry
                        2419200         ; Expire
                         604800 )       ; Negative Cache TTL
;
        IN      NS      ns1.ziralink.com.
        IN      NS      ns2.ziralink.com.

; name servers - A
ns1.ziralink.com.               IN      A       127.0.0.1
ns2.ziralink.com.               IN      A       127.0.0.1

@       IN      A       127.0.0.1
*       IN      A       127.0.0.1