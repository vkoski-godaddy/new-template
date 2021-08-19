#!/bin/bash
joe=$(cat /etc/hostname)
fred='this is a test'
alex=$(awk -F' is a ' '{print $1}' <<<"$fred")
mike=$(awk -F' is a ' '{print $2}' <<<"$fred")
ruben="$mike $alex $fred $joe"
echo "$ruben"
