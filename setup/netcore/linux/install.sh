#!/bin/bash

if [ $(/usr/bin/id -u) -ne 0 ]
then
    echo "$0 is not running as root. Try using sudo."
    exit
fi

WEXFLOW_SERVICE="/opt/wexflow/wexflow.service"
WEXFLOW_SERVICE_SYSTEMD="/etc/systemd/system/wexflow.service"

if [ -e  $WEXFLOW_SERVICE_SYSTEMD ]
then
    echo "Updating wexflow systemd service..."
    sudo cp $WEXFLOW_SERVICE $WEXFLOW_SERVICE_SYSTEMD
    sudo systemctl daemon-reload
    sudo systemctl restart wexflow.service
    sudo systemctl status wexflow.service --no-pager
else
    echo "Installing wexflow systemd service..."
    sudo cp $WEXFLOW_SERVICE $WEXFLOW_SERVICE_SYSTEMD
    sudo systemctl enable wexflow.service
    sudo systemctl start wexflow.service
    sudo systemctl status wexflow.service --no-pager
fi
