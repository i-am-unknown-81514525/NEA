msbuild || true
cd ui/stdin_handler 
cargo build --release || true
cd ../.. 
mkdir build 
cp NEA/bin/Debug/* build || true
cp ui/bin/Debug/* build || true
cp ui/stdin_handler/target/release/* build || true