require("CandleForStrategy")

IsStop = false;

Stack = {};             -- Массив для стека
Stack.idx_for_add = 1;  -- Индекс для добавления следующей, или первой записи
Stack.idx_for_get = 1;  -- Индекс для изъятия следующей, или первой записи
Stack.count = 0;        -- Количество находящихся в стеке записей
Stack.max = 1000;       -- Максимально возможное количество записей в стеке (при переполнении старые записи будут замещаться новыми) 
trans_id             = os.time()       -- ID транзакции
EXPIRY_DATE          = 'GTC'         -- Срок действия стоп-заявки: 'TODAY' - до окончания текущей торговой сессии, 'GTC' -до отмены, или время в формате 'ГГГГММДД'
LyftStop = {};

-- function CheckPrice()
	-- i = #Stack / 7
	
	-- for j = 1, i,1 do
		-- local tmp = PriceForCheck(Stack[(j - 1) * 7 + 1], Stack[(j - 1) * 7 + 2], Stack[(j - 1) * 7 + 3], Stack[(j - 1) * 7 + 4], Stack[(j - 1) * 7 + 5], Stack[(j - 1) * 7 + 6], Stack[(j - 1) * 7 + 7])
		-- if(tmp) then
		-- table.remove(Stack,(j - 1) * 7 + 1);
		-- table.remove(Stack,(j - 1) * 7 + 2);
		-- table.remove(Stack,(j - 1) * 7 + 3);
		-- table.remove(Stack,(j - 1) * 7 + 4);
		-- table.remove(Stack,(j - 1) * 7 + 5);
		-- table.remove(Stack,(j - 1) * 7 + 6);
		-- table.remove(Stack,(j - 1) * 7 + 7);
		-- CheckPrice()
		-- break;
		-- end
	-- end
-- end

-- function PriceForCheck(
-- account, class_code, sec_code, id, price, operation, idStop
-- )
-- message(tostring(account))
-- message(tostring(class_code))
-- message(tostring(sec_code))

	-- ds = CreateDataSource(class_code,sec_code,60)
	-- ds:SetEmptyCallback()
	-- local tmp = false;
	-- if(operation == 'S') then
		-- if(ds:C(ds:Size()) > tonumber(price)) then 
			-- tmp = true;
		-- end
	-- else
		-- if(ds:C(ds:Size()) < tonumber(price)) then
			-- tmp = true;
		-- end
	-- end
	-- if (GetTotalnet(account, class_code, sec_code) == 0) then
      -- KillOrder(class_code, sec_code, id)
	  -- Kill_SO(class_code, sec_code, id, idStop)
	  -- return true;
	-- end
	-- return false;
-- end

function main()

	local Candle = "";
	local Command = "";
	while not IsStop do
		--message(tostring('i work'));
		-- if(Stack.count > 0) then
			-- CheckPrice()
		-- end
		Command = tostring(QluaCSharpConnector.GetCommand());
		if Command ~= "" then
			message(tostring(Command));
			local temp = {}
			local i = 1
			local str = ""
			for j = 1, #Command,1 do
			
				if(Command:sub(j,j) == ';') then
					temp[i] = str
					i = i + 1
					str = ""
				else
				
				str = str..Command:sub(j,j)
				
					if(j == #Command) then
						temp[i] = str
						str = ""
					end
				end
			end
			message('after split'..' '..tostring(#temp));
			if(temp[1] == 'GetCandle')	then		
				RequestCandle(temp);
			elseif(temp[1] == 'SetOrder') then
				SetOrderLimit(temp);
			elseif(temp[1] == 'Set_SL') then
				SetStopLoss(temp);
			elseif(temp[1] == 'SetTP_SL') then
				SetTakeProfitStopLoss(temp);
			-- elseif(temp[1] == 'KillOrder' and (GetTotalnet(temp[5], temp[2], temp[3]) == 0)) then
				-- KillOrder(temp[2], temp[3], temp[4]);
			-- elseif(temp[1] == 'KillStopOrder' and (GetTotalnet(temp[5], temp[2], temp[3]) == 0)) then
				-- Kill_SO(temp[2], temp[3], temp[4]);
			-- elseif(temp[1] == 'CheckTable') then
				-- CheckTable(temp[2], temp[3]);
			end
			Command = "";
		end
	sleep(1000)
	end
end

function CheckTable(sec_code, timeFrame)
	local str = 'orders'..';'..sec_code..';'..timeFrame..';'
	local haveOrder = false;
	-- Перебирает строки таблицы "Позиции по клиентским счетам (фьючерсы)", ищет Текущие чистые позиции по инструменту "sec_code"
for i = 0,getNumberOf("orders") - 1 do
   -- ЕСЛИ строка по нужному инструменту и 
   if (getItem("orders",i).sec_code == sec_code) and  (bit.test(getItem("orders",i).flags, 0)) then
	message('^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^')
	  haveOrder = true;
      str = str..tostring(getItem("orders",i).order_num)
   end;
end;
if not haveOrder then
	str = str..'DeleteAllOrders'
end
for i = 0,getNumberOf("stop_orders") - 1 do
	--message('my sec_code =   '..sec_code)
    --message(tostring(getItem("stop_orders",i).sec_code))
	--message(tostring(bit.test(getItem("stop_orders",i).flags, 0)))
   -- ЕСЛИ строка по нужному инструменту и 
   if (getItem("stop_orders",i).sec_code == sec_code) and (bit.test(getItem("stop_orders",i).flags, 0)) then
   
      str = str..';'..tostring(getItem("stop_orders",i).order_num)
	  QluaCSharpConnector.SendCandle(str)
	  return 0;
   end;
end;
str = str..';'..'DeleteAllStopProfitOrders'
QluaCSharpConnector.SendCandle(str)
end

CheckBit = function(flags, _bit)
   -- Проверяет, что переданные аргументы являются числами
   if type(flags) ~= "number" then error("Ошибка!!! Checkbit: 1-й аргумент не число!") end
   if type(_bit) ~= "number" then error("Ошибка!!! Checkbit: 2-й аргумент не число!") end
 
   if _bit == 0 then _bit = 0x1
   elseif _bit == 1 then _bit = 0x2
   elseif _bit == 2 then _bit = 0x4
   elseif _bit == 3 then _bit  = 0x8
   elseif _bit == 4 then _bit = 0x10
   elseif _bit == 5 then _bit = 0x20
   elseif _bit == 6 then _bit = 0x40
   elseif _bit == 7 then _bit  = 0x80
   elseif _bit == 8 then _bit = 0x100
   elseif _bit == 9 then _bit = 0x200
   elseif _bit == 10 then _bit = 0x400
   elseif _bit == 11 then _bit = 0x800
   elseif _bit == 12 then _bit  = 0x1000
   elseif _bit == 13 then _bit = 0x2000
   elseif _bit == 14 then _bit  = 0x4000
   elseif _bit == 15 then _bit  = 0x8000
   elseif _bit == 16 then _bit = 0x10000
   elseif _bit == 17 then _bit = 0x20000
   elseif _bit == 18 then _bit = 0x40000
   elseif _bit == 19 then _bit = 0x80000
   elseif _bit == 20 then _bit = 0x100000
   end
 
   if bit.band(flags,_bit ) == _bit then return true
   else return false end
end

function SetOrderLimit(temp)

	table.remove(temp,1);
	
	i = #temp / 6
	
	for j = 1, i,1 do
		--AddToStack(temp[(j - 1) * 6 + 1])
		--message('add to stack'.. ' '..tostring(temp[(j - 1) * 6 + 1]))
		--AddToStack(temp[(j - 1) * 6 + 2])
		--message('add to stack'.. ' '..tostring(temp[(j - 1) * 6 + 2]))
		--AddToStack(temp[(j - 1) * 6 + 3])
		--message('add to stack'.. ' '..tostring(temp[(j - 1) * 6 + 3]))
		SetOrder(temp[(j - 1) * 6 + 1], temp[(j - 1) * 6 + 2], temp[(j - 1) * 6 + 3], temp[(j - 1) * 6 + 4], temp[(j - 1) * 6 + 5], temp[(j - 1) * 6 + 6])
		
	end
end

function RequestCandle(temp)

	table.remove(temp,1);
	message('in request'..' '..tostring(#temp));
	i = #temp / 4
	
	for j = 1, i,1 do
		GetCandle(temp[(j - 1) * 4 + 1], temp[(j - 1) * 4 + 2], temp[(j - 1) * 4 + 3], temp[(j - 1) * 4 + 4])
	end
end


function SetTakeProfitStopLoss(temp)
	table.remove(temp,1);
	
	i = #temp / 8
	
	for j = 1, i,1 do
		SetPrifitLoss(temp[(j - 1) * 8 + 1], temp[(j - 1) * 8 + 2], temp[(j - 1) * 8 + 3], temp[(j - 1) * 8 + 4], temp[(j - 1) * 8 + 5], temp[(j - 1) * 8 + 6], temp[(j - 1) * 8 + 7], temp[(j - 1) * 8 + 8])
	end
end

function SetStopLoss(temp)

	table.remove(temp,1);
	i = #temp / 6
	
	for j = 1, i,1 do
		Set_SL(temp[(j - 1) * 6 + 1], temp[(j - 1) * 6 + 2], temp[(j - 1) * 6 + 3], temp[(j - 1) * 6 + 4], temp[(j - 1) * 6 + 5], temp[(j - 1) * 6 + 6])
	end
end

function GetCandle(class,security,interval,count) 
	message('in getCandle');
	local countCandle = -1;
	local ds;
	ds = CreateDataSource(class,security,tonumber(interval))
	ds:SetEmptyCallback()
	message('ds Calculate');
	if(ds:Size() == 0) then
		local tmpcount = 0
		local iteration = 0;
		repeat
			if(ds ~= nil and iteration ~= 0) then
				countCandle = ds:Size();  
			end
			ds = CreateDataSource(class,security,tonumber(interval))
			ds:SetEmptyCallback()
			if(iteration == 0) then
				sleep(20000)
			else
				sleep(3000)
			end
			if(ds:Size() == 0) then
				tmpcount = tmpcount + 1
			end
			iteration = iteration + 1
		until (countCandle == ds:Size() or tmpcount == 10)
	end
	
	
	local str = security..';'..interval..';'
	if tonumber(count) == 0 then
	-- Отправляет существующие свечи в sharp
		if (tonumber(interval) == 0) then
		
			str = str..count..';'..ds:Size()
			
			message(tostring('Send Count ticks'));
			QluaCSharpConnector.SendCandle(str)
		else
			for j=1,ds:Size(),1 do
				--message('ostalos'..tostring(ds:Size() - j));
				str = str..os.date('%Y-%m-%d %H:%M:%S', os.time(ds:T(j)))..';'..ds:O(j)..';'..ds:H(j)..';'..ds:L(j)..';'..ds:C(j)..';'..ds:V(j)..';'
			end
			message(tostring('try Send Candle'));
			QluaCSharpConnector.SendCandle(str)
		end
	else
		if(tonumber(interval) == 0) then 
			message(tostring('in interval == 0 ADD'));
			message(tostring(ds:Size() - tonumber(count)));
			
			str = str..count..';'
			for j= tonumber(count),ds:Size(),1 do
				str = str..os.date('%Y-%m-%d %H:%M:%S', os.time(ds:T(j)))..';'..ds:O(j)..';'..ds:V(j)..';'
			end
			message(tostring('try Send Candle for Tick'));
			QluaCSharpConnector.SendCandle(str)
		else
			local temp = tonumber(count);
			for j=temp,ds:Size(),1 do
				str = str..os.date('%Y-%m-%d %H:%M:%S', os.time(ds:T(j)))..';'..ds:O(j)..';'..ds:H(j)..';'..ds:L(j)..';'..ds:C(j)..';'..ds:V(j)..';'
			end
		QluaCSharpConnector.SendCandle(str)
		end
	end
end

-- Добавляет запись в стек
function AddToStack(NewEntry)
   -- Добавляет запись в стек
   Stack[Stack.idx_for_add] = NewEntry;
   -- Корректирует счетчик находящихся в стеке записей
   if Stack.count < Stack.max then Stack.count = Stack.count + 1; end;
   -- Увеличивает индекс для добавления следующей записи
   Stack.idx_for_add = Stack.idx_for_add + 1;
   -- Если индекс больше максимально допустимого, то следующая запись будет добавляться в начало стека
   if Stack.idx_for_add > Stack.max then Stack.idx_for_add = 1; end;
   -- Если изъятие записей отстало от записи (новая запись переписала старую), то увеличивает индекс для изъятия следующей записи
   if Stack.idx_for_add - Stack.idx_for_get == 1 and Stack.count > 1 -- смещение внутри стека
      then Stack.idx_for_get = Stack.idx_for_get + 1;
   -- Добавил в конец, когда индекс для изъятия тоже был в конце и количество не равно 0
   elseif Stack.idx_for_get - Stack.idx_for_add == Stack.max - 1 and Stack.count > 1
      then Stack.idx_for_get = 1; 
   end;
end;

-- Извлекает запись из стека
function GetFromStack()
   local OldInxForGet = Stack.idx_for_get;
   if Stack.count == 0 then return nil; end;
   -- Уменьшает количество записей на 1
   Stack.count = Stack.count - 1;
   -- Корректирует, если это была единственная запись
   if Stack.count == -1 then
      Stack.count = 0;
      Stack.idx_for_get = Stack.idx_for_add; -- Выравнивает индексы
   else -- Если еще есть записи 
      -- Сдвигает индекс изъятия на 1 вправо
      Stack.idx_for_get = Stack.idx_for_get + 1;
      -- Корректирует, если достигнут конец
      if Stack.idx_for_get > Stack.max then Stack.idx_for_get = 1; end;
   end;
   return Stack[OldInxForGet];
end;

SetOrder = function(
   account,	   -- Код счета
   class_code, -- Код класса
   sec_code,   -- Код инструмента
   price,      -- Цена заявки
   operation,  -- Операция ('B' - buy, 'S' - sell)
   qty         -- Количество 
)
local str = 'Order'..';'..class_code..';'..sec_code..';'..price..';'..operation..';'..qty..';'
   -- Выставляет заявку
   -- Получает ID для следующей транзакции
   trans_id = trans_id + 1
   --AddToStack(trans_id);
   -- Заполняет структуру для отправки транзакции
   local idTransaction = tostring(trans_id)
   local T = {}
   T['TRANS_ID']   = idTransaction     -- Номер транзакции
   T['ACCOUNT']    = account                -- Код счета
   T['CLASSCODE']  = class_code             -- Код класса
   T['SECCODE']    = sec_code               -- Код инструмента
   T['ACTION']     = 'NEW_ORDER'            -- Тип транзакции ('NEW_ORDER' - новая заявка)      
   T['TYPE']       = 'L'                    -- Тип ('L' - лимитированная, 'M' - рыночная)
   T['OPERATION']  = operation              -- Операция ('B' - buy, или 'S' - sell)
   T['PRICE']      = GetCorrectPrice(price, class_code, sec_code) -- Цена
   T['QUANTITY']   = tostring(qty)          -- Количество
 
   -- Отправляет транзакцию
   local Res = sendTransaction(T)
   -- Если при отправке транзакции возникла ошибка
   if Res ~= '' then
      -- Выводит сообщение об ошибке
      message('Error transaction open/close: '..Res)
   end
   str = str..idTransaction
   --QluaCSharpConnector.SendCandle(str);
end

-- Выставляет стоп-лимит заявку
Set_SL = function(
   account,	      -- Код счета
   class_code,    -- Код класса
   sec_code,      -- Код инструмента
   operation,     -- Операция ('B' - buy, 'S' - sell)
   stop_price,    -- Цена Стоп-Лосса
   qty            -- Количество в лотах
)
   -- Получает ID для следующей транзакции
   trans_id = trans_id + 1
   -- Вычисляет цену, по которой выставится заявка при срабатывании стопа
   local price = stop_price - 1*PriceStep
   if operation == 'B' then price = stop_price + 2*PriceStep end
   -- Заполняет структуру для отправки транзакции на Стоп-лосс
   local T = {}
   T['TRANS_ID']           = tostring(trans_id)
   T['CLASSCODE']          = class_code
   T['SECCODE']            = sec_code
   T['ACCOUNT']            = account
   T['ACTION']             = 'NEW_STOP_ORDER'               -- Тип заявки      
   T['OPERATION']          = operation                      -- Операция ('B' - покупка(BUY), 'S' - продажа(SELL))
   T['QUANTITY']           = tostring(qty)                  -- Количество в лотах
   T['STOPPRICE']          = GetCorrectPrice(stop_price, class_code, sec_code)    -- Цена Стоп-Лосса
   T['PRICE']              = GetCorrectPrice(price, class_code, sec_code)         -- Цена, по которой выставится заявка при срабатывании Стоп-Лосса (для рыночной заявки по акциям должна быть 0)
   T['EXPIRY_DATE']        = EXPIRY_DATE                    -- 'TODAY', 'GTC', или время
 
   -- Отправляет транзакцию
   local Res = sendTransaction(T)
   -- Если при отправке транзакции возникла ошибка
   if Res ~= '' then
      -- Выводит ошибку
      message('Error transaction stop loss: '..Res)
   end
end

-- Выставляет "Тейк профит и Стоп лимит" заявку
SetPrifitLoss = function(
	account,	  -- Код счета
   class_code, 	  -- Код класса
   sec_code,      -- Код инструмента
   operation,     -- Операция ('B', или 'S')
   pos_price,     -- Цена позиции, на которую выставляется стоп-заявка
   qty,           -- Количество лотов
   profit_size,   -- Размер профита в шагах цены
   stop_size      -- Размер стопа в шагах цены
)
local tmp;


pos_price = string.gsub(tostring(pos_price), ',', '.');
pos_price = GetCorrectPrice(tonumber(pos_price), class_code, sec_code);
pos_price = string.gsub(tostring(pos_price), ',', '.');
pos_price = tonumber(pos_price)
   -- Получает минимальный шаг цены
   local PriceStep = tonumber(getParamEx(class_code, sec_code, "SEC_PRICE_STEP").param_value)
   -- Получает максимально возможную цену заявки
   local PriceMax = tonumber(getParamEx(class_code,  sec_code, 'PRICEMAX').param_value)
   -- Получает минимально возможную цену заявки
   local PriceMin = tonumber(getParamEx(class_code,  sec_code, 'PRICEMIN').param_value)
   
   if(operation == 'B') then 
tmp = pos_price - PriceStep * 2 * stop_size
elseif(operation == 'S') then
tmp = pos_price + PriceStep * 2 * stop_size
end
--AddToStack(tmp)
--AddToStack(operation)
TransactionId = tostring(trans_id + 1)
--AddToStack(trans_id);

local str = 'StopProfitOrder'..';'..class_code..';'..sec_code..';'..tostring(pos_price)..';'..operation..';'..tostring(qty)..';'..TransactionId

   -- Заполняет структуру для отправки транзакции на Стоп-лосс и Тэйк-профит
   local T = {}
   T['TRANS_ID']              = TransactionId
   T['CLASSCODE']             = class_code
   T['SECCODE']               = sec_code
   T['ACCOUNT']               = account
   T['ACTION']                = 'NEW_STOP_ORDER'                                    -- Тип заявки      
   T['STOP_ORDER_KIND']       = 'TAKE_PROFIT_AND_STOP_LIMIT_ORDER'                  -- Тип стоп-заявки
   T['OPERATION']             = operation                                           -- Операция ('B' - покупка(BUY), 'S' - продажа(SELL))   
   T['QUANTITY']              = tostring(qty)                                       -- Количество в лотах
 
   -- Вычисляет цену профита
   local stopprice = 0
   if operation == 'B' then
      stopprice = pos_price - profit_size*PriceStep
      if PriceMin ~= nil and PriceMin ~= 0 and stopprice < PriceMin then
         stopprice = PriceMin
      end
   elseif operation == 'S' then
      stopprice = pos_price + profit_size*PriceStep
      if PriceMax ~= nil and PriceMax ~= 0 and stopprice > PriceMax then
         stopprice = PriceMax
      end
   end
   T['STOPPRICE']             = GetCorrectPrice(stopprice, class_code, sec_code)                          -- Цена Тэйк-Профита
   T['OFFSET']                = '0'                                                 -- отступ
   T['OFFSET_UNITS']          = 'PRICE_UNITS'                                       -- в шагах цены
   local spread = 1*PriceStep
   if operation == 'B' then
      if PriceMax ~= nil and PriceMax ~= 0 and stopprice + spread > PriceMax then
         spread = PriceMax - stopprice - 1*PriceStep
      end
   elseif operation == 'S' then
      if PriceMin ~= nil and PriceMin ~= 0 and stopprice - spread < PriceMin then
         spread = stopprice - PriceMin - 1*PriceStep
      end
   end
   T['SPREAD']                = GetCorrectPrice(spread, class_code, sec_code)                             -- Защитный спред
   T['SPREAD_UNITS']          = 'PRICE_UNITS'                                       -- в шагах цены
   T['MARKET_TAKE_PROFIT']    = 'NO'                                                -- 'YES', или 'NO'
 
   -- Вычисляет цену стопа
   local stopprice2 = 0
   if operation == 'B' then
      stopprice2 = pos_price + stop_size*PriceStep
      if PriceMax ~= nil and PriceMax ~= 0 and stopprice2 > PriceMax then
         stopprice2 = PriceMax
      end
   elseif operation == 'S' then
      stopprice2 = pos_price - stop_size*PriceStep
      if PriceMin ~= nil and PriceMin ~= 0 and stopprice2 < PriceMin then
         stopprice2 = PriceMin
      end
   end
   T['STOPPRICE2']            = GetCorrectPrice(stopprice2, class_code, sec_code)                         -- Цена Стоп-Лосса
   -- Вычисляет цену, по которой выставится заявка при срабатывании стопа
   local price = 0
   if operation == 'B' then
      price = stopprice2 + 1*PriceStep
      if PriceMax ~= nil and PriceMax ~= 0 and price > PriceMax then
         price = PriceMax
      end
   elseif operation == 'S' then
      price = stopprice2 - 1*PriceStep
      if PriceMin ~= nil and PriceMin ~= 0 and price < PriceMin then
         price = PriceMin
      end
   end
   T['PRICE']                 = GetCorrectPrice(price, class_code, sec_code)                              -- Цена, по которой выставится заявка при срабатывании Стоп-Лосса (для рыночной заявки по акциям должна быть 0)
   T['MARKET_STOP_LIMIT']     = 'NO'                                                -- 'YES', или 'NO'
   T['EXPIRY_DATE']           = EXPIRY_DATE                                         -- 'TODAY', 'GTC', или время
   T['IS_ACTIVE_IN_TIME']     = 'NO'                                                -- Признак действия заявки типа «Тэйк-профит и стоп-лимит» в течение определенного интервала времени. Значения «YES» или «NO»
 
   -- Отправляет транзакцию
   local Res = sendTransaction(T)
   if Res ~= '' then
      message('Error set TAKE_PROFIT_AND_STOP_LIMIT_ORDER: '..Res)
   end
   --QluaCSharpConnector.SendCandle(str);
end

-- Приводит переданную цену к требуемому для транзакции по инструменту виду
GetCorrectPrice = function(price, class_code, sec_code) -- STRING
   -- Получает точность цены по инструменту
   local scale = getSecurityInfo(class_code, sec_code).scale
   -- Получает минимальный шаг цены инструмента
   local PriceStep = tonumber(getParamEx(class_code, sec_code, "SEC_PRICE_STEP").param_value)
   -- Если после запятой должны быть цифры
   if scale > 0 then
      price = tostring(price)
      -- Ищет в числе позицию запятой, или точки
      local dot_pos = price:find('.')
      local comma_pos = price:find(',')
      -- Если передано целое число
      if dot_pos == nil and comma_pos == nil then
         -- Добавляет к числу ',' и необходимое количество нулей и возвращает результат
         price = price..','
         for i=1,scale do price = price..'0' end
         return price
      else -- передано вещественное число         
         -- Если нужно, заменяет запятую на точку 
		 
         if comma_pos ~= nil then price =  string.gsub(price, ',', '.') end
		 
         -- Округляет число до необходимого количества знаков после запятой
         price = math_round(tonumber(price), scale)
		 --message('price after first math_round'..' '..tostring(price));
         -- Корректирует на соответствие шагу цены
         price = math_round(price/PriceStep)*PriceStep
		 --message('price after second math_round'..' '..tostring(price));
         price = string.gsub(tostring(price),'[\.]+', ',')
		 --message('price after gsub'..' '..tostring(price));
         return price
      end
   else -- После запятой не должно быть цифр
	  price = tonumber(price)
	 -- message(tostring(price));
	 -- message(tostring('tyt2'));
      -- Корректирует на соответствие шагу цены
      price = math_round(price/PriceStep)*PriceStep
      return tostring(math.floor(price))
   end
end

-- Округляет число до указанной точности
math_round = function(num, idp)		-- num is number
    -- message('in math_round'.. ' '..tostring(num));
	 num = tonumber(num);
  local mult = 10^(idp or 0)
  return math.floor(num * mult + 0.5) / mult
end

-- Получает текущую чистую позицию по инструменту
GetTotalnet = function(
   account,	   -- Код счета
   class_code, -- Код класса
   sec_code    -- Код инструмента
)
   -- ФЬЮЧЕРСЫ, ОПЦИОНЫ
   if class_code == 'SPBFUT' or class_code == 'SPBOPT' then
      for i = 0,getNumberOf('futures_client_holding') - 1 do
         local futures_client_holding = getItem('futures_client_holding',i)
         if futures_client_holding.sec_code == sec_code then
            return futures_client_holding.totalnet
         end
      end
   -- АКЦИИ
   elseif class_code == 'TQBR' or class_code == 'QJSIM' then
      for i = 0,getNumberOf('depo_limits') - 1 do
         local depo_limit = getItem("depo_limits", i)
         if depo_limit.sec_code == sec_code
         and depo_limit.trdaccid == account
         and depo_limit.limit_kind == LIMIT_KIND then         
            return depo_limit.currentbal
         end
      end
   end
 
   -- Если позиция по инструменту в таблице не найдена, возвращает 0
   return 0
end

-- Снимает заявку по ее номеру
KillOrder = function(
   class_code,  -- Код класса
   sec_code,    -- Код инструмента
   order_num    -- number order
   )
   trans_id = trans_id + 1
   local T = {
      ['CLASSCODE']  = class_code,
      ['SECCODE']    = sec_code,
      ['TRANS_ID']   = tostring(trans_id),
      ['ACTION']     = 'KILL_ORDER',
      ['ORDER_KEY']  = tostring(order_num)
   }
   
   -- Отправляет транзакцию
   local Res = sendTransaction(T)
   -- Если при отправке транзакции возникла ошибка
   if Res ~= '' then
      -- Выводит сообщение об ошибке
      message('Error transaction kill order: '..Res)
   end
end

-- Снимает стоп-заявку
Kill_SO = function(
   class_code,  -- Код класса
   sec_code,    -- Код инструмента
   stop_order_num    -- Номер снимаемой стоп-заявки
)
stop_order_num = tonumber(stop_order_num)
RUN = true;
message('stoporder - '..stop_order_num)
   -- Находит стоп-заявку (30 сек. макс.)
   local index = 0
   local start_sec = os.time()
   local find_so = false
   while RUN and not find_so and os.time() - start_sec < 30 do
      for i=getNumberOf('stop_orders')-1,0,-1 do
         local stop_order = getItem('stop_orders', i)
         if stop_order.order_num == stop_order_num then
            -- Если стоп-заявка уже была исполнена (не активна)
            if not bit.test(stop_order.flags, 0) then
               return false
            end
            index = i
            find_so = true
            break
         end
      end
   end
   if not find_so then
      message('Error: do not find stop-order!')
      return false
   end
 
   -- Получает ID для следующей транзакции
   trans_id = trans_id + 1
   -- Заполняет структуру для отправки транзакции на снятие стоп-заявки
   local T = {}
   T['TRANS_ID']            = tostring(trans_id)
   T['CLASSCODE']           = class_code
   T['SECCODE']             = sec_code
   T['ACTION']              = 'KILL_STOP_ORDER'        -- Тип заявки 
   T['STOP_ORDER_KEY']      = tostring(stop_order_num) -- Номер стоп-заявки, снимаемой из торговой системы
 
   -- Отправляет транзакцию
   local Res = sendTransaction(T)
   -- Если при отправке транзакции возникла ошибка
   if Res ~= '' then
      -- Выводит ошибку
      message('Error delete stop-order : '..Res)
      return false
   end   
 
   -- Ожидает когда стоп-заявка перестанет быть активна (30 сек. макс.)
   start_sec = os.time()
   local active = true
   while RUN and os.time() - start_sec < 30 do
      local stop_order = getItem('stop_orders', index)
      -- Если стоп-заявка не активна
      if not bit.test(stop_order.flags, 0) then
         -- Если стоп-заявка успела исполниться
         if not bit.test(stop_order.flags, 1) then
            return false
         end
         active = false
         break
      end
      sleep(10)
   end
   if active then
      message('Unknown error in time delete stop-prder')
      return false
   end
 
   return true
end

--- Функция вызывается терминалом QUIK при завершении пользователем скрипта
function OnStop(s)
   IsStop = true;
end