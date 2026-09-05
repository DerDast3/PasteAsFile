#!/usr/bin/env bash
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
echo "Cleaned: removed all bin/ and obj/ folders."
