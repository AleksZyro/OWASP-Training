FROM nginxinc/nginx-unprivileged:1.27-alpine

ARG CHALLENGE
COPY challenges/shared/ /usr/share/nginx/html/
COPY challenges/${CHALLENGE}/ /usr/share/nginx/html/
