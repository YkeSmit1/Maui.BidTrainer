#pragma once

#include <string>
#include <vector>

enum class Player { West, North, East, South };

struct HandCharacteristic
{
	std::string hand {};
	
	std::vector<int> suitLengths;
	bool isBalanced = false;
	bool isSemiBalanced = false;
	bool isReverse = false;
	int lengthFirstSuit = 0;
	int lengthSecondSuit = 0;

	int firstSuit = -1;
	int secondSuit = -1;
	int lowestSuit = -1;
	int highestSuit = -1;

	int hcp = 0;
	std::vector<bool> controls{};

	static bool GetHasControl(const std::string& suit);
	void Initialize(const std::string& hand);
	explicit HandCharacteristic(const std::string& hand);
	HandCharacteristic() = default;
};