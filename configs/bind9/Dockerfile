FROM ubuntu/bind9

RUN apt update \
  && apt install -y \
  bind9-doc \
  dnsutils \
  geoip-bin \
  mariadb-server \
  net-tools

# Copy configuration files
COPY ./named.conf.options /etc/bind/
COPY ./named.conf.local /etc/bind/
COPY ./db.ziralink.com /etc/bind/zones/

# Expose Ports
EXPOSE 53/tcp
EXPOSE 53/udp
EXPOSE 953/tcp

ENV BIND9_USER=bind
ENV TZ=UTC

# Start the Name Service
CMD ["/usr/sbin/named", "-g", "-c", "/etc/bind/named.conf", "-u", "bind"]