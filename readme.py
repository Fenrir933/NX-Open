import subprocess
import sys

def main() :
	if sys.argv[0] in "readme":
		args = [sys.argv[1], sys.argv[2]]
	else:
		args = [sys.argv[0], sys.argv[1]]

	subprocess.Popen(args)


if __name__ == "__main__":
	main()
