#!/bin/bash

SMARTIES_BASE_DIR="$(dirname $(dirname $(python -c 'import os,sys;print os.path.realpath(sys.argv[1])' $0)))"
echo $SMARTIES_BASE_DIR
WAFLOCK=${WAFLOCK:-.lock-wafbuild}
WAF_BUILD_DIRNAME="${WAFLOCK:9}"
WAF_BUILD_PATH="${SMARTIES_BASE_DIR}/${WAF_BUILD_DIRNAME}"
echo $WAF_BUILD_DIR
